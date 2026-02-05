namespace Nexus.API.UseCases.Diagrams.DTOs;

/// <summary>
/// Request DTO for creating a new diagram
/// </summary>
public class CreateDiagramRequest
{
  public string Title { get; set; } = string.Empty;
  public string DiagramType { get; set; } = "Flowchart"; // Flowchart, Network, UML, ERD, Custom
  public DiagramCanvasDto? Canvas { get; set; }
  public Guid? CollectionId { get; set; }
}

/// <summary>
/// Request DTO for updating diagram properties
/// </summary>
public class UpdateDiagramRequest
{
  public string? Title { get; set; }
  public DiagramCanvasDto? Canvas { get; set; }
}

/// <summary>
/// Request DTO for adding an element to a diagram
/// </summary>
public class AddElementRequest
{
  public string ShapeType { get; set; } = "Rectangle";
  public PointDto Position { get; set; } = new();
  public SizeDto Size { get; set; } = new();
  public string? Text { get; set; }
  public ElementStyleDto? Style { get; set; }
  public Guid? LayerId { get; set; }
  public int ZIndex { get; set; }
}

/// <summary>
/// Request DTO for updating an existing element
/// </summary>
public class UpdateElementRequest
{
  public PointDto? Position { get; set; }
  public SizeDto? Size { get; set; }
  public string? Text { get; set; }
  public ElementStyleDto? Style { get; set; }
  public int? ZIndex { get; set; }
  public bool? IsLocked { get; set; }
}

/// <summary>
/// Request DTO for adding a connection between elements
/// </summary>
public class AddConnectionRequest
{
  public Guid SourceElementId { get; set; }
  public Guid TargetElementId { get; set; }
  public string ConnectionType { get; set; } = "Arrow";
  public string? Label { get; set; }
  public ConnectionStyleDto? Style { get; set; }
}

/// <summary>
/// Request DTO for updating an existing connection
/// </summary>
public class UpdateConnectionRequest
{
  public string? Label { get; set; }
  public ConnectionStyleDto? Style { get; set; }
}

/// <summary>
/// Request DTO for adding a new layer
/// </summary>
public class AddLayerRequest
{
  public string Name { get; set; } = string.Empty;
  public bool IsVisible { get; set; } = true;
  public bool IsLocked { get; set; } = false;
}

/// <summary>
/// Request DTO for updating layer properties
/// </summary>
public class UpdateLayerRequest
{
  public string? Name { get; set; }
  public int? Order { get; set; }
  public bool? IsVisible { get; set; }
  public bool? IsLocked { get; set; }
}

/// <summary>
/// Full diagram response DTO
/// </summary>
public class DiagramDto
{
  public Guid DiagramId { get; set; }
  public string Title { get; set; } = string.Empty;
  public string DiagramType { get; set; } = string.Empty;
  public DiagramCanvasDto Canvas { get; set; } = new();
  public List<DiagramElementDto> Elements { get; set; } = new();
  public List<DiagramConnectionDto> Connections { get; set; } = new();
  public List<LayerDto> Layers { get; set; } = new();
  public UserInfoDto CreatedBy { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Lightweight diagram DTO for list views
/// </summary>
public class DiagramListItemDto
{
  public Guid DiagramId { get; set; }
  public string Title { get; set; } = string.Empty;
  public string DiagramType { get; set; } = string.Empty;
  public int ElementCount { get; set; }
  public int ConnectionCount { get; set; }
  public UserInfoDto CreatedBy { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Paged result DTO for diagram lists
/// </summary>
public class DiagramPagedResultDto
{
  public List<DiagramListItemDto> Items { get; set; } = new();
  public int Page { get; set; }
  public int PageSize { get; set; }
  public int TotalCount { get; set; }
  public int TotalPages { get; set; }
  public bool HasNextPage { get; set; }
  public bool HasPreviousPage { get; set; }
}

/// <summary>
/// Diagram element DTO
/// </summary>
public class DiagramElementDto
{
  public Guid ElementId { get; set; }
  public string ShapeType { get; set; } = string.Empty;
  public PointDto Position { get; set; } = new();
  public SizeDto Size { get; set; } = new();
  public string? Text { get; set; }
  public ElementStyleDto Style { get; set; } = new();
  public Guid? LayerId { get; set; }
  public int ZIndex { get; set; }
  public bool IsLocked { get; set; }
  public Dictionary<string, object>? CustomProperties { get; set; }
}

/// <summary>
/// Diagram connection DTO
/// </summary>
public class DiagramConnectionDto
{
  public Guid ConnectionId { get; set; }
  public Guid SourceElementId { get; set; }
  public Guid TargetElementId { get; set; }
  public string ConnectionType { get; set; } = string.Empty;
  public string? Label { get; set; }
  public ConnectionStyleDto Style { get; set; } = new();
}

/// <summary>
/// Layer DTO
/// </summary>
public class LayerDto
{
  public Guid LayerId { get; set; }
  public string Name { get; set; } = string.Empty;
  public int Order { get; set; }
  public bool IsVisible { get; set; }
  public bool IsLocked { get; set; }
}

/// <summary>
/// Canvas configuration DTO
/// </summary>
public class DiagramCanvasDto
{
  public decimal Width { get; set; } = 1920;
  public decimal Height { get; set; } = 1080;
  public string BackgroundColor { get; set; } = "#FFFFFF";
  public int GridSize { get; set; } = 20;
}

/// <summary>
/// Point DTO for element positions
/// </summary>
public class PointDto
{
  public decimal X { get; set; }
  public decimal Y { get; set; }
}

/// <summary>
/// Size DTO for element dimensions
/// </summary>
public class SizeDto
{
  public decimal Width { get; set; }
  public decimal Height { get; set; }
}

/// <summary>
/// Element style DTO
/// </summary>
public class ElementStyleDto
{
  public string FillColor { get; set; } = "#FFFFFF";
  public string StrokeColor { get; set; } = "#000000";
  public int StrokeWidth { get; set; } = 2;
  public int FontSize { get; set; } = 14;
  public string FontFamily { get; set; } = "Arial";
  public decimal Opacity { get; set; } = 1.0m;
  public decimal Rotation { get; set; } = 0m;
}

/// <summary>
/// Connection style DTO
/// </summary>
public class ConnectionStyleDto
{
  public string StrokeColor { get; set; } = "#000000";
  public int StrokeWidth { get; set; } = 2;
  public string? StrokeDashArray { get; set; }
}

/// <summary>
/// User information DTO
/// </summary>
public class UserInfoDto
{
  public Guid UserId { get; set; }
  public string Username { get; set; } = string.Empty;

  public UserInfoDto()
  {
  }

  public UserInfoDto(Guid userId, string username)
  {
    UserId = userId;
    Username = username;
  }
}
