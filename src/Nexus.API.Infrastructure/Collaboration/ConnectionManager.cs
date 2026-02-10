using System.Collections.Concurrent;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.Infrastructure.Collaboration;

/// <summary>
/// In-memory implementation of connection manager
/// For production scale-out, consider using Redis
/// </summary>
public class ConnectionManager : IConnectionManager
{
    // SessionId -> UserId -> List of ConnectionIds
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, HashSet<string>>> _sessionConnections = new();

    // ConnectionId -> SessionId (for cleanup)
    private readonly ConcurrentDictionary<string, Guid> _connectionToSession = new();

    // ConnectionId -> UserId (for cleanup)
    private readonly ConcurrentDictionary<string, Guid> _connectionToUser = new();

    // SessionId -> UserId -> CursorPosition
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, int>> _cursorPositions = new();

    // SessionId -> Set of UserIds who are typing
    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _typingUsers = new();

    public Task AddToSessionAsync(string connectionId, Guid sessionId, Guid userId)
    {
        // Add to session connections
        var userConnections = _sessionConnections.GetOrAdd(sessionId, _ => new ConcurrentDictionary<Guid, HashSet<string>>());
        var connections = userConnections.GetOrAdd(userId, _ => new HashSet<string>());

        lock (connections)
        {
            connections.Add(connectionId);
        }

        // Track connection mappings
        _connectionToSession[connectionId] = sessionId;
        _connectionToUser[connectionId] = userId;

        return Task.CompletedTask;
    }

    public Task RemoveFromSessionAsync(string connectionId, Guid sessionId)
    {
        if (!_connectionToUser.TryGetValue(connectionId, out var userId))
        {
            return Task.CompletedTask;
        }

        // Remove from session connections
        if (_sessionConnections.TryGetValue(sessionId, out var userConnections))
        {
            if (userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                }

                // If user has no more connections, remove user from session
                if (connections.Count == 0)
                {
                    userConnections.TryRemove(userId, out _);

                    // Clean up cursor position
                    if (_cursorPositions.TryGetValue(sessionId, out var cursors))
                    {
                        cursors.TryRemove(userId, out _);
                    }

                    // Clean up typing status
                    if (_typingUsers.TryGetValue(sessionId, out var typingSet))
                    {
                        lock (typingSet)
                        {
                            typingSet.Remove(userId);
                        }
                    }
                }
            }

            // If session has no more connections, remove session
            if (userConnections.IsEmpty)
            {
                _sessionConnections.TryRemove(sessionId, out _);
                _cursorPositions.TryRemove(sessionId, out _);
                _typingUsers.TryRemove(sessionId, out _);
            }
        }

        // Remove connection mappings
        _connectionToSession.TryRemove(connectionId, out _);
        _connectionToUser.TryRemove(connectionId, out _);

        return Task.CompletedTask;
    }

    public List<string> GetUserConnections(Guid sessionId, Guid userId)
    {
        if (_sessionConnections.TryGetValue(sessionId, out var userConnections))
        {
            if (userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    return connections.ToList();
                }
            }
        }
        return new List<string>();
    }

    public List<Guid> GetSessionUsers(Guid sessionId)
    {
        if (_sessionConnections.TryGetValue(sessionId, out var userConnections))
        {
            return userConnections.Keys.ToList();
        }
        return new List<Guid>();
    }

    public int GetSessionConnectionCount(Guid sessionId)
    {
        if (_sessionConnections.TryGetValue(sessionId, out var userConnections))
        {
            return userConnections.Values.Sum(connections =>
            {
                lock (connections)
                {
                    return connections.Count;
                }
            });
        }
        return 0;
    }

    public bool IsUserConnected(Guid sessionId, Guid userId)
    {
        if (_sessionConnections.TryGetValue(sessionId, out var userConnections))
        {
            if (userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    return connections.Count > 0;
                }
            }
        }
        return false;
    }

    public void UpdateCursorPosition(Guid sessionId, Guid userId, int position)
    {
        var cursors = _cursorPositions.GetOrAdd(sessionId, _ => new ConcurrentDictionary<Guid, int>());
        cursors[userId] = position;
    }

    public int? GetCursorPosition(Guid sessionId, Guid userId)
    {
        if (_cursorPositions.TryGetValue(sessionId, out var cursors))
        {
            if (cursors.TryGetValue(userId, out var position))
            {
                return position;
            }
        }
        return null;
    }

    public Dictionary<Guid, int> GetAllCursorPositions(Guid sessionId)
    {
        if (_cursorPositions.TryGetValue(sessionId, out var cursors))
        {
            return new Dictionary<Guid, int>(cursors);
        }
        return new Dictionary<Guid, int>();
    }

    public void UpdateTypingStatus(Guid sessionId, Guid userId, bool isTyping)
    {
        var typingSet = _typingUsers.GetOrAdd(sessionId, _ => new HashSet<Guid>());

        lock (typingSet)
        {
            if (isTyping)
            {
                typingSet.Add(userId);
            }
            else
            {
                typingSet.Remove(userId);
            }
        }
    }

    public List<Guid> GetTypingUsers(Guid sessionId)
    {
        if (_typingUsers.TryGetValue(sessionId, out var typingSet))
        {
            lock (typingSet)
            {
                return typingSet.ToList();
            }
        }
        return new List<Guid>();
    }

    public async Task CleanupConnectionAsync(string connectionId)
    {
        // Get session for this connection
        if (_connectionToSession.TryGetValue(connectionId, out var sessionId))
        {
            await RemoveFromSessionAsync(connectionId, sessionId);
        }
    }

    public Guid? GetConnectionSession(string connectionId)
    {
        if (_connectionToSession.TryGetValue(connectionId, out var sessionId))
        {
            return sessionId;
        }
        return null;
    }
}
