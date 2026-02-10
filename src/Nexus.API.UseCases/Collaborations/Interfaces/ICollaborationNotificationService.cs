using Nexus.API.UseCases.Collaboration.DTOs;

namespace Nexus.API.UseCases.Collaboration.Interfaces;

/// <summary>
/// Abstraction for broadcasting real-time collaboration notifications.
/// Defined in UseCases layer - implemented in Web layer using SignalR.
/// Follows Dependency Inversion: UseCases defines the contract,
/// the outer layer (Web) provides the implementation.
/// </summary>
public interface ICollaborationNotificationService
{
    // ─── Session Events ───────────────────────────────────────────────────────

    /// <summary>
    /// Notifies users watching a resource that a collaboration session has started.
    /// Broadcasts to the resource group (not the session group, which doesn't exist yet).
    /// </summary>
    Task NotifySessionStartedAsync(
        Guid sessionId,
        Guid resourceId,
        string resourceType,
        Guid startedByUserId,
        DateTime startedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies all participants and resource watchers that a session has ended.
    /// Broadcasts to both the session group and the resource group.
    /// </summary>
    Task NotifySessionEndedAsync(
        Guid sessionId,
        Guid resourceId,
        string resourceType,
        Guid endedByUserId,
        DateTime endedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies existing session participants that a new participant has been added (via REST).
    /// Note: When the participant connects via WebSocket, the hub will send ParticipantJoined.
    /// </summary>
    Task NotifyParticipantAddedAsync(
        Guid sessionId,
        Guid userId,
        string role,
        int activeParticipantCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies existing session participants that a participant has been removed (via REST).
    /// Note: When the participant disconnects from WebSocket, the hub will send ParticipantLeft.
    /// </summary>
    Task NotifyParticipantRemovedAsync(
        Guid sessionId,
        Guid userId,
        int activeParticipantCount,
        CancellationToken cancellationToken = default);

    // ─── Comment Events ───────────────────────────────────────────────────────

    /// <summary>
    /// Notifies session participants and resource watchers that a comment was added.
    /// </summary>
    Task NotifyCommentAddedAsync(
        CommentNotificationDto comment,
        Guid? sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies session participants and resource watchers that a comment was updated.
    /// </summary>
    Task NotifyCommentUpdatedAsync(
        CommentNotificationDto comment,
        Guid? sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies session participants and resource watchers that a comment was deleted.
    /// </summary>
    Task NotifyCommentDeletedAsync(
        CommentNotificationDto comment,
        Guid? sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a targeted notification to the author of a parent comment when someone replies to it.
    /// Only sent when the reply author is different from the parent comment author.
    /// </summary>
    Task NotifyReplyReceivedAsync(
        Guid parentCommentAuthorUserId,
        Guid replyCommentId,
        Guid parentCommentId,
        Guid resourceId,
        string resourceType,
        Guid replyAuthorUserId,
        string replyText,
        DateTime createdAt,
        CancellationToken cancellationToken = default);
}
