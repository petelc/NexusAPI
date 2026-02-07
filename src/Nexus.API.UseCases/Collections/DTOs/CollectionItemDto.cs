namespace Nexus.API.UseCases.Collections.DTOs;

public class CollectionItemDto
{
  public Guid CollectionItemId { get; set; }
  public string ItemType { get; set; } = string.Empty;
  public Guid ItemReferenceId { get; set; }
  public string? ItemTitle { get; set; }
  public int Order { get; set; }
  public Guid AddedBy { get; set; }
  public string? AddedByUsername { get; set; }
  public DateTime AddedAt { get; set; }
}
