using Nexus.API.Core.Aggregates.CollaborationAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Enums;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for collaboration operations
/// </summary>
public interface ICollaborationRepository
{
    // ========================================================================
    // COLLABORATION SESSIONS
    // ========================================================================

    /// <summary>
    /// Get session by ID
    /// </summary>
    Task<CollaborationSession?> GetSessionByIdAsync(SessionId sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active sessions for a resource
    /// </summary>
    Task<IEnumerable<CollaborationSession>> GetActiveSessionsByResourceAsync(
        ResourceType resourceType,
        ResourceId resourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get sessions where user is a participant
    /// </summary>
    Task<IEnumerable<CollaborationSession>> GetUserSessionsAsync(
        ParticipantId userId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add session
    /// </summary>
    Task<CollaborationSession> AddSessionAsync(CollaborationSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update session
    /// </summary>
    Task UpdateSessionAsync(CollaborationSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete session
    /// </summary>
    Task DeleteSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default);

    // ========================================================================
    // COMMENTS
    // ========================================================================

    /// <summary>
    /// Get comment by ID
    /// </summary>
    Task<Comment?> GetCommentByIdAsync(CommentId commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get comments for a resource
    /// </summary>
    Task<IEnumerable<Comment>> GetResourceCommentsAsync(
        ResourceType resourceType,
        ResourceId resourceId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get comments for a session
    /// </summary>
    Task<IEnumerable<Comment>> GetSessionCommentsAsync(
        SessionId sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add comment
    /// </summary>
    Task<Comment> AddCommentAsync(Comment comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update comment
    /// </summary>
    Task UpdateCommentAsync(Comment comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete comment
    /// </summary>
    Task DeleteCommentAsync(CommentId commentId, CancellationToken cancellationToken = default);

    // ========================================================================
    // SESSION CHANGES
    // ========================================================================

    /// <summary>
    /// Get changes for a session
    /// </summary>
    Task<IEnumerable<SessionChange>> GetSessionChangesAsync(
        SessionId sessionId,
        DateTime? since = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add change
    /// </summary>
    Task<SessionChange> AddChangeAsync(SessionChange change, CancellationToken cancellationToken = default);

    // ========================================================================
    // SESSION PARTICIPANTS
    // ========================================================================

    /// <summary>
    /// Get active participants for a session
    /// </summary>
    Task<IEnumerable<SessionParticipant>> GetActiveParticipantsAsync(
        SessionId sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user is participant in any active session for a resource
    /// </summary>
    Task<bool> IsUserInActiveSessionAsync(
        ParticipantId userId,
        ResourceType resourceType,
        ResourceId resourceId,
        CancellationToken cancellationToken = default);
}
