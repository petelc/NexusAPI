using Ardalis.Result;
using Nexus.API.Core.Aggregates.ResourcePermissions;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.UseCases.Permissions.Commands;

// --- Command ---

public record RevokePermissionCommand(
    Guid PermissionId,
    Guid RequestingUserId) : IRequest<Result>;

// --- Handler ---

public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand, Result>
{
    private readonly IPermissionRepository _permissionRepository;

    public RevokePermissionCommandHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository
            ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    public async Task<Result> Handle(
        RevokePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetByIdAsync(
            command.PermissionId, cancellationToken);

        if (permission is null)
            return Result.NotFound("Permission not found.");

        if (permission.IsOwner)
            return Result.Invalid(
                new ValidationError
                {
                    ErrorMessage = "Owner permissions cannot be revoked. Transfer ownership before revoking access."
                });

        var requesterPermission = await _permissionRepository.GetByResourceAndUserAsync(
            permission.ResourceType, permission.ResourceId, command.RequestingUserId, cancellationToken);

        var requesterIsAdminOrAbove = requesterPermission is not null &&
                                      requesterPermission.CanManagePermissions;

        var requesterIsOriginalGranter = permission.GrantedBy == command.RequestingUserId;

        if (!requesterIsAdminOrAbove && !requesterIsOriginalGranter)
            return Result.Unauthorized();

        await _permissionRepository.DeleteAsync(permission, cancellationToken);

        return Result.Success();
    }
}
