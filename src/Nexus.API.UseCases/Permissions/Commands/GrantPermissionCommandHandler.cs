using Ardalis.Result;
using Nexus.API.Core.Aggregates.ResourcePermissions;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Permissions.DTOs;

namespace Nexus.API.UseCases.Permissions.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public record GrantPermissionCommand(
    string ResourceType,
    Guid ResourceId,
    Guid TargetUserId,
    string Level,
    Guid GrantedByUserId,
    DateTime? ExpiresAt) : IRequest<Result<PermissionDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public class GrantPermissionCommandHandler : IRequestHandler<GrantPermissionCommand, Result<PermissionDto>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GrantPermissionCommandHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository
            ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    public async Task<Result<PermissionDto>> Handle(
        GrantPermissionCommand command,
        CancellationToken cancellationToken)
    {
        // Parse resource type
        if (!Enum.TryParse<ResourceType>(command.ResourceType, ignoreCase: true, out var resourceType))
            return Result<PermissionDto>.Invalid(
                new ValidationError { ErrorMessage = $"Invalid resource type '{command.ResourceType}'. Valid values: Document, Diagram, CodeSnippet." });

        // Parse permission level
        if (!Enum.TryParse<PermissionLevel>(command.Level, ignoreCase: true, out var level))
            return Result<PermissionDto>.Invalid(
                new ValidationError { ErrorMessage = $"Invalid permission level '{command.Level}'. Valid values: Viewer, Commenter, Editor, Admin, Owner." });

        // Owner grants should be rare and explicit — disallow granting Owner via API
        // (owners are set at resource creation time)
        if (level == PermissionLevel.Owner)
            return Result<PermissionDto>.Invalid(
                new ValidationError { ErrorMessage = "Owner permission cannot be granted via this endpoint. Transfer ownership through the resource-specific endpoint." });

        // Check for duplicate grant
        var existing = await _permissionRepository.GetByResourceAndUserAsync(
            resourceType, command.ResourceId, command.TargetUserId, cancellationToken);

        if (existing is not null && existing.IsValid)
        {
            // Update level instead of creating duplicate
            existing.ChangeLevel(level);
            existing.SetExpiry(command.ExpiresAt);
            await _permissionRepository.UpdateAsync(existing, cancellationToken);
            return Result<PermissionDto>.Success(existing.ToDto());
        }

        // Create new grant
        var permission = ResourcePermission.Grant(
            resourceType,
            command.ResourceId,
            command.TargetUserId,
            level,
            command.GrantedByUserId,
            command.ExpiresAt);

        await _permissionRepository.AddAsync(permission, cancellationToken);

        return Result<PermissionDto>.Success(permission.ToDto());
    }
}
