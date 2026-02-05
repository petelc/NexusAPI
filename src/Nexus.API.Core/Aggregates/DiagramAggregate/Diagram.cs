using Ardalis.GuardClauses;
using Nexus.API.Core.Enums;
using Nexus.API.Core.Events;
using Nexus.API.Core.Exceptions;
using Nexus.API.Core.ValueObjects;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Aggregates.DiagramAggregate;

/// <summary>
/// Diagram aggregate root
/// Manages visual diagrams with elements, connections, and layers
/// </summary>
public class Diagram : EntityBase<DiagramId>, IAggregateRoot
{
  public Title Title { get; private set; } = null!;
  public DiagramType DiagramType { get; private set; }
  public Guid CreatedBy { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }
  public DiagramCanvas Canvas { get; private set; } = null!;
  public bool IsDeleted { get; private set; }
  public DateTime? DeletedAt { get; private set; }

  private readonly List<DiagramElement> _elements = new();
  public IReadOnlyCollection<DiagramElement> Elements => _elements.AsReadOnly();

  private readonly List<DiagramConnection> _connections = new();
  public IReadOnlyCollection<DiagramConnection> Connections => _connections.AsReadOnly();

  private readonly List<Layer> _layers = new();
  public IReadOnlyCollection<Layer> Layers => _layers.AsReadOnly();

  private Diagram() { } // EF Core

  private Diagram(
    DiagramId id,
    Title title,
    DiagramType diagramType,
    Guid createdBy,
    DiagramCanvas canvas)
  {
    Id = id;
    Title = Guard.Against.Null(title, nameof(title));
    DiagramType = diagramType;
    CreatedBy = Guard.Against.Default(createdBy, nameof(createdBy));
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
    Canvas = canvas ?? DiagramCanvas.CreateDefault();
    IsDeleted = false;
  }

  /// <summary>
  /// Factory method to create a new diagram
  /// </summary>
  public static Diagram Create(
    Title title,
    DiagramType diagramType,
    Guid createdBy,
    DiagramCanvas? canvas = null)
  {
    var diagram = new Diagram(
      DiagramId.CreateNew(),
      title,
      diagramType,
      createdBy,
      canvas ?? DiagramCanvas.CreateDefault()
    );

    // Create default layer
    diagram._layers.Add(Layer.CreateDefault());

    diagram.RegisterDomainEvent(new DiagramCreatedEvent(diagram.Id, createdBy));
    return diagram;
  }

  /// <summary>
  /// Update diagram title
  /// </summary>
  public void UpdateTitle(Title newTitle)
  {
    Title = Guard.Against.Null(newTitle, nameof(newTitle));
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Resize the canvas
  /// </summary>
  public void ResizeCanvas(double width, double height)
  {
    Canvas = Canvas.Resize(width, height);
    UpdatedAt = DateTime.UtcNow;
  }

  #region Element Management

  /// <summary>
  /// Add an element to the diagram
  /// </summary>
  public void AddElement(DiagramElement element)
  {
    Guard.Against.Null(element, nameof(element));

    if (!Canvas.IsWithinBounds(element.Position, element.Size))
      throw new DomainException("Element is outside canvas bounds");

    _elements.Add(element);
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new DiagramElementAddedEvent(Id, element.Id));
  }

  /// <summary>
  /// Remove an element from the diagram
  /// Cannot remove elements that have connections
  /// </summary>
  public void RemoveElement(ElementId elementId)
  {
    Guard.Against.Null(elementId, nameof(elementId));

    // Check if element has connections
    if (_connections.Any(c => c.SourceElementId.Value == elementId.Value ||
                              c.TargetElementId.Value == elementId.Value))
      throw new DomainException("Cannot remove element with connections. Remove connections first.");

    var element = _elements.FirstOrDefault(e => e.Id.Value == elementId.Value);
    if (element == null)
      throw new DomainException("Element not found");

    _elements.Remove(element);
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new DiagramElementRemovedEvent(Id, elementId));
  }

  /// <summary>
  /// Update element position and size
  /// </summary>
  public void UpdateElement(ElementId elementId, Point position, Size size)
  {
    var element = GetElement(elementId);

    if (!Canvas.IsWithinBounds(position, size))
      throw new DomainException("Element would be outside canvas bounds");

    element.UpdatePosition(position);
    element.UpdateSize(size);
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new DiagramElementUpdatedEvent(Id, elementId));
  }

  /// <summary>
  /// Update element text
  /// </summary>
  public void UpdateElementText(ElementId elementId, string? text)
  {
    var element = GetElement(elementId);
    element.UpdateText(text);
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Update element style
  /// </summary>
  public void UpdateElementStyle(ElementId elementId, ElementStyle style)
  {
    var element = GetElement(elementId);
    element.UpdateStyle(style);
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Get an element by ID
  /// </summary>
  public DiagramElement GetElement(ElementId elementId)
  {
    return _elements.FirstOrDefault(e => e.Id.Value == elementId.Value)
      ?? throw new DomainException("Element not found");
  }

  #endregion

  #region Connection Management

  /// <summary>
  /// Add a connection between two elements
  /// </summary>
  public void AddConnection(DiagramConnection connection)
  {
    Guard.Against.Null(connection, nameof(connection));

    // Validate source and target elements exist
    if (!_elements.Any(e => e.Id.Value == connection.SourceElementId.Value))
      throw new DomainException("Source element not found");
    if (!_elements.Any(e => e.Id.Value == connection.TargetElementId.Value))
      throw new DomainException("Target element not found");

    _connections.Add(connection);
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new DiagramConnectionAddedEvent(Id, connection.Id));
  }

  /// <summary>
  /// Remove a connection
  /// </summary>
  public void RemoveConnection(ConnectionId connectionId)
  {
    var connection = _connections.FirstOrDefault(c => c.Id.Value == connectionId.Value);
    if (connection == null)
      throw new DomainException("Connection not found");

    _connections.Remove(connection);
    UpdatedAt = DateTime.UtcNow;

    RegisterDomainEvent(new DiagramConnectionRemovedEvent(Id, connectionId));
  }

  /// <summary>
  /// Update connection label
  /// </summary>
  public void UpdateConnectionLabel(ConnectionId connectionId, string? label)
  {
    var connection = GetConnection(connectionId);
    connection.UpdateLabel(label);
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Update connection style
  /// </summary>
  public void UpdateConnectionStyle(ConnectionId connectionId, ConnectionStyle style)
  {
    var connection = GetConnection(connectionId);
    connection.UpdateStyle(style);
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Get a connection by ID
  /// </summary>
  public DiagramConnection GetConnection(ConnectionId connectionId)
  {
    return _connections.FirstOrDefault(c => c.Id.Value == connectionId.Value)
      ?? throw new DomainException("Connection not found");
  }

  #endregion

  #region Layer Management

  /// <summary>
  /// Add a new layer
  /// </summary>
  public Layer AddLayer(string name)
  {
    var order = _layers.Count;
    var layer = Layer.Create(name, order);
    _layers.Add(layer);
    UpdatedAt = DateTime.UtcNow;
    return layer;
  }

  /// <summary>
  /// Remove a layer
  /// Cannot remove if elements are on the layer
  /// </summary>
  public void RemoveLayer(LayerId layerId)
  {
    // Check if any elements are on this layer
    if (_elements.Any(e => e.LayerId?.Value == layerId.Value))
      throw new DomainException("Cannot remove layer with elements. Move elements first.");

    var layer = _layers.FirstOrDefault(l => l.Id.Value == layerId.Value);
    if (layer == null)
      throw new DomainException("Layer not found");

    _layers.Remove(layer);
    ReorderLayers();
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Reorder layers after removal
  /// </summary>
  private void ReorderLayers()
  {
    for (int i = 0; i < _layers.Count; i++)
    {
      _layers[i].UpdateOrder(i);
    }
  }

  /// <summary>
  /// Get a layer by ID
  /// </summary>
  public Layer GetLayer(LayerId layerId)
  {
    return _layers.FirstOrDefault(l => l.Id.Value == layerId.Value)
      ?? throw new DomainException("Layer not found");
  }

  #endregion

  #region Soft Delete

  /// <summary>
  /// Soft delete the diagram
  /// </summary>
  public void Delete()
  {
    IsDeleted = true;
    DeletedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Restore a soft-deleted diagram
  /// </summary>
  public void Restore()
  {
    IsDeleted = false;
    DeletedAt = null;
    UpdatedAt = DateTime.UtcNow;
  }

  #endregion
}
