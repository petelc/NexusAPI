using Nexus.API.Core.ValueObjects;
using MediatR;

namespace Nexus.API.UseCases.Collaboration.Queries;

/// <summary>
/// Query to get comments for a specific resource
/// </summary>
public record GetResourceCommentsQuery(
    string ResourceType,
    ResourceId ResourceId,
    bool IncludeDeleted = false) : IRequest<Result<IEnumerable<CommentResponseDto>>>;
