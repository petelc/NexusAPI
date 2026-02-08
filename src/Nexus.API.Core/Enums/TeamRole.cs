namespace Nexus.API.Core.Enums;

/// <summary>
/// Defines the role of a team member
/// </summary>
public enum TeamRole
{
    /// <summary>
    /// Basic team member - can view content
    /// </summary>
    Member = 1,

    /// <summary>
    /// Team administrator - can manage members and content
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Team owner - full control including deleting team
    /// </summary>
    Owner = 3
}
