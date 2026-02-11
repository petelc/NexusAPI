using Nexus.API.Core.ValueObjects;
using MediatR;

namespace Nexus.API.UseCases.Collaboration.Queries;

/// <summary>
/// Query to get a comment by ID
/// </summary>
public record GetCommentByIdQuery(CommentId CommentId) : IRequest<Result<CommentResponseDto>>;
