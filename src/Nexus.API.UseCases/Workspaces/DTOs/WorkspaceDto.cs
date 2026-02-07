namespace Nexus.API.UseCases.Workspaces.DTOs;

/// <summary>
/// DTO for workspace data
/// </summary>
public class WorkspaceDto
{
  public Guid WorkspaceId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Guid TeamId { get; set; }
  public Guid CreatedBy { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int MemberCount { get; set; }
  public List<WorkspaceMemberDto> Members { get; set; } = new();
}
