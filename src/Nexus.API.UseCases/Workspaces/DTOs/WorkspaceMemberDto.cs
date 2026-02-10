namespace Nexus.API.UseCases.Workspaces.DTOs;

/// <summary>
/// DTO for workspace member data
/// </summary>
public class WorkspaceMemberDto
{
  public Guid MemberId { get; set; }
  public Guid UserId { get; set; }
  public string Role { get; set; } = string.Empty;
  public DateTime JoinedAt { get; set; }
  public Guid? InvitedBy { get; set; }
  public bool IsActive { get; set; }
}
