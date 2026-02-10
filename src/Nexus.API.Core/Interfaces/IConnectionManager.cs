namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Manages SignalR connections and session group memberships
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// Add a connection to a session group
    /// </summary>
    Task AddToSessionAsync(string connectionId, Guid sessionId, Guid userId);

    /// <summary>
    /// Remove a connection from a session group
    /// </summary>
    Task RemoveFromSessionAsync(string connectionId, Guid sessionId);

    /// <summary>
    /// Get all connection IDs for a user in a session
    /// </summary>
    List<string> GetUserConnections(Guid sessionId, Guid userId);

    /// <summary>
    /// Get all users in a session
    /// </summary>
    List<Guid> GetSessionUsers(Guid sessionId);

    /// <summary>
    /// Get total connection count for a session
    /// </summary>
    int GetSessionConnectionCount(Guid sessionId);

    /// <summary>
    /// Check if a user is connected to a session
    /// </summary>
    bool IsUserConnected(Guid sessionId, Guid userId);

    /// <summary>
    /// Store cursor position for a user in a session
    /// </summary>
    void UpdateCursorPosition(Guid sessionId, Guid userId, int position);

    /// <summary>
    /// Get cursor position for a user in a session
    /// </summary>
    int? GetCursorPosition(Guid sessionId, Guid userId);

    /// <summary>
    /// Get all cursor positions in a session
    /// </summary>
    Dictionary<Guid, int> GetAllCursorPositions(Guid sessionId);

    /// <summary>
    /// Update typing status for a user in a session
    /// </summary>
    void UpdateTypingStatus(Guid sessionId, Guid userId, bool isTyping);

    /// <summary>
    /// Get typing users in a session
    /// </summary>
    List<Guid> GetTypingUsers(Guid sessionId);

    /// <summary>
    /// Clean up all data for a connection (on disconnect)
    /// </summary>
    Task CleanupConnectionAsync(string connectionId);

    /// <summary>
    /// Get session ID for a connection
    /// </summary>
    Guid? GetConnectionSession(string connectionId);
}
