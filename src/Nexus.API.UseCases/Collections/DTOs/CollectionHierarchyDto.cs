namespace Nexus.API.UseCases.Collections.DTOs;

public class CollectionHierarchyDto
{
  public Guid CollectionId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Icon { get; set; }
  public string? Color { get; set; }
  public int HierarchyLevel { get; set; }
  public int ItemCount { get; set; }
  public List<CollectionHierarchyDto> Children { get; set; } = new();
}
