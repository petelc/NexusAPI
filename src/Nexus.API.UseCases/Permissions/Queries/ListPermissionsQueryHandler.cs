using Ardalis.Result;
using Nexus.API.Core.Aggregates.ResourcePermissions;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Permissions.DTOs;

namespace Nexus.API.UseCases.Permissions.Queries;

// --- Query ---

public record ListPermissionsQuery(
    string ResourceType,
    Guid ResourceId,
    Guid RequestingUserId) : IRequest<Result<IReadOnlyList<PermissionDto>>>;

// --- Handler ---

public class ListPermissionsQueryHandler : IRequestHandler<ListPermissionsQuery, Result<IReadOnlyList<PermissionDto>>>
{
    private readonly IPermissionRepository _permissionRepository;

    public ListPermissionsQueryHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository
            ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(
        ListPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ResourceType>(query.ResourceType, ignoreCase: true, out var resourceType))
            return Result<IReadOnlyList<PermissionDto>>.Invalid(
                new ValidationError { ErrorMessage = $"Invalid resource type '{query.ResourceType}'. Valid values: Document, Diagram, CodeSnippet." });

        // Caller must have at least Viewer access to list permissions on a resource
        var requesterPermission = await _permissionRepository.GetByResourceAndUserAsync(
            resourceType, query.ResourceId, query.RequestingUserId, cancellationToken);

        if (requesterPermission is null || !requesterPermission.IsValid)
            return Result<IReadOnlyList<PermissionDto>>.Unauthorized();

        var permissions = await _permissionRepository.GetByResourceAsync(
            resourceType, query.ResourceId, cancellationToken);

        var dtos = permissions
            .Select(p => p.ToDto())
            .ToList();

        return Result<IReadOnlyList<PermissionDto>>.Success(dtos);
    }
}
