using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Data;

namespace Nexus.API.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for collaboration operations
/// </summary>
public class CollaborationRepository : ICollaborationRepository
{
    private readonly AppDbContext _context;

    public CollaborationRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ========================================================================
    // COLLABORATION SESSIONS
    // ========================================================================

    public async Task<CollaborationSession?> GetSessionByIdAsync(
        SessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CollaborationSessions
            .Include(s => s.Participants)
            .Include(s => s.Changes)
            .Include(s => s.Comments)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<IEnumerable<CollaborationSession>> GetActiveSessionsByResourceAsync(
        ResourceType resourceType,
        ResourceId resourceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CollaborationSessions
            .Include(s => s.Participants)
            .Where(s => s.ResourceType == resourceType
                     && s.ResourceId == resourceId
                     && s.IsActive)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CollaborationSession>> GetUserSessionsAsync(
        ParticipantId userId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.CollaborationSessions
            .Include(s => s.Participants)
            .Where(s => s.Participants.Any(p => p.UserId == userId));

        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        return await query
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CollaborationSession> AddSessionAsync(
        CollaborationSession session,
        CancellationToken cancellationToken = default)
    {
        await _context.CollaborationSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task UpdateSessionAsync(
        CollaborationSession session,
        CancellationToken cancellationToken = default)
    {
        _context.CollaborationSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSessionAsync(
        SessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.CollaborationSessions
            .FindAsync(new object[] { sessionId }, cancellationToken);

        if (session != null)
        {
            _context.CollaborationSessions.Remove(session);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // ========================================================================
    // COMMENTS
    // ========================================================================

    public async Task<Comment?> GetCommentByIdAsync(
        CommentId commentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetResourceCommentsAsync(
        ResourceType resourceType,
        ResourceId resourceId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Comments
            .Include(c => c.Replies)
            .Where(c => c.ResourceType == resourceType
                     && c.ResourceId == resourceId
                     && c.ParentCommentId == null); // Only root comments

        if (!includeDeleted)
        {
            query = query.Where(c => !c.IsDeleted);
        }

        return await query
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetSessionCommentsAsync(
        SessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Include(c => c.Replies)
            .Where(c => c.SessionId == sessionId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Comment> AddCommentAsync(
        Comment comment,
        CancellationToken cancellationToken = default)
    {
        await _context.Comments.AddAsync(comment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return comment;
    }

    public async Task UpdateCommentAsync(
        Comment comment,
        CancellationToken cancellationToken = default)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCommentAsync(
        CommentId commentId,
        CancellationToken cancellationToken = default)
    {
        var comment = await _context.Comments
            .FindAsync(new object[] { commentId }, cancellationToken);

        if (comment != null)
        {
            comment.Delete(); // Soft delete
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // ========================================================================
    // SESSION CHANGES
    // ========================================================================

    public async Task<IEnumerable<SessionChange>> GetSessionChangesAsync(
        SessionId sessionId,
        DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SessionChanges
            .Where(c => c.SessionId == sessionId);

        if (since.HasValue)
        {
            query = query.Where(c => c.Timestamp > since.Value);
        }

        return await query
            .OrderBy(c => c.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<SessionChange> AddChangeAsync(
        SessionChange change,
        CancellationToken cancellationToken = default)
    {
        await _context.SessionChanges.AddAsync(change, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return change;
    }

    // ========================================================================
    // SESSION PARTICIPANTS
    // ========================================================================

    public async Task<IEnumerable<SessionParticipant>> GetActiveParticipantsAsync(
        SessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SessionParticipants
            .Where(p => p.SessionId == sessionId && p.LeftAt == null)
            .OrderBy(p => p.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsUserInActiveSessionAsync(ParticipantId userId, ResourceType resourceType, ResourceId resourceId, CancellationToken cancellationToken = default)
    {
        return _context.SessionParticipants
            .AnyAsync(p => p.UserId == userId
                        && p.LeftAt == null
                        && p.Session.ResourceType == resourceType
                        && p.Session.ResourceId == resourceId
                        && p.Session.IsActive,
                        cancellationToken);
    }

}
