namespace Nexus.API.Core.Enums;

/// <summary>
/// Roles available for workspace members
/// </summary>
public enum WorkspaceMemberRole
{
  /// <summary>
  /// Can view content only
  /// </summary>
  Viewer = 1,

  /// <summary>
  /// Can view and edit content
  /// </summary>
  Editor = 2,

  /// <summary>
  /// Can manage members and settings
  /// </summary>
  Admin = 3,

  /// <summary>
  /// Full control over workspace
  /// </summary>
  Owner = 4
}
