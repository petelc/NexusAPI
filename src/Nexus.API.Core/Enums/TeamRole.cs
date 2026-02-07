namespace Nexus.API.Core.Enums;

/// <summary>
/// Enumeration of possible roles a team member can have, defining their permissions and access levels within the team and its workspaces
/// </summary>
public enum TeamRole
{
    Owner,      // Full access to all team resources and settings
    Admin,      // Can manage team members and settings, but may have some restrictions compared to Owner
    Member,     // Can access and contribute to team resources, but cannot manage team settings
}