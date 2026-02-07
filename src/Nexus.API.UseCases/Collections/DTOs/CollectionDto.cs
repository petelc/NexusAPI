namespace Nexus.API.UseCases.Collections.DTOs;

public class CollectionDto
{
  public Guid CollectionId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Guid? ParentCollectionId { get; set; }
  public Guid WorkspaceId { get; set; }
  public Guid CreatedBy { get; set; }
  public string? CreatedByUsername { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string? Icon { get; set; }
  public string? Color { get; set; }
  public int OrderIndex { get; set; }
  public int HierarchyLevel { get; set; }
  public string HierarchyPath { get; set; } = string.Empty;
  public int ItemCount { get; set; }
  public List<CollectionItemDto> Items { get; set; } = new();
}
