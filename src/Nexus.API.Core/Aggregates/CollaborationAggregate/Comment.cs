using Nexus.API.Core.Enums;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.CollaborationAggregate;

/// <summary>
/// Comment entity
/// Represents a comment on a document or diagram, can be part of a collaboration session
/// </summary>
public class Comment : EntityBase<CommentId>
{
    // Properties
    public Guid? SessionId { get; private set; }
    public ResourceType ResourceType { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid UserId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public int? Position { get; private set; }
    public Guid? ParentCommentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Navigation properties
    public CollaborationSession? Session { get; private set; }
    public Comment? ParentComment { get; private set; }
    private readonly List<Comment> _replies = new();
    public IReadOnlyCollection<Comment> Replies => _replies.AsReadOnly();

    // Private constructor for EF Core
    private Comment() { }

    /// <summary>
    /// Factory method to create a new comment
    /// </summary>
    public static Comment Create(
        Guid? sessionId,
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        string text,
        int? position = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Comment text cannot be empty", nameof(text));
        }

        if (text.Length > 2000)
        {
            throw new ArgumentException("Comment text cannot exceed 2000 characters", nameof(text));
        }

        return new Comment
        {
            Id = CommentId.CreateNew(),
            SessionId = sessionId,
            ResourceType = resourceType,
            ResourceId = resourceId,
            UserId = userId,
            Text = text,
            Position = position,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create a reply to an existing comment
    /// </summary>
    public static Comment CreateReply(
        CommentId parentCommentId,
        ResourceType resourceType,
        Guid resourceId,
        Guid userId,
        string text,
        Guid? sessionId = null)
    {
        var reply = Create(sessionId, resourceType, resourceId, userId, text);
        reply.ParentCommentId = parentCommentId.Value;
        return reply;
    }

    /// <summary>
    /// Update comment text
    /// </summary>
    public void UpdateText(string newText)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot update deleted comment");
        }

        if (string.IsNullOrWhiteSpace(newText))
        {
            throw new ArgumentException("Comment text cannot be empty", nameof(newText));
        }

        if (newText.Length > 2000)
        {
            throw new ArgumentException("Comment text cannot exceed 2000 characters", nameof(newText));
        }

        Text = newText;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft delete the comment
    /// </summary>
    public void Delete()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Comment is already deleted");
        }

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add a reply to this comment
    /// </summary>
    public void AddReply(Comment reply)
    {
        if (reply.ParentCommentId != Id.Value)
        {
            throw new InvalidOperationException("Reply's parent comment ID does not match");
        }

        _replies.Add(reply);
    }
}
