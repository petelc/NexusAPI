namespace Nexus.API.Core.Models;

/// <summary>
/// File metadata model
/// </summary>
public class FileMetadata
{
  public string FileName { get; set; } = string.Empty;
  public string ContentType { get; set; } = string.Empty;
  public long SizeInBytes { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime LastModified { get; set; }
}
