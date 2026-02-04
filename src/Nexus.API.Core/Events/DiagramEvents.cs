using Nexus.API.Core.ValueObjects;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Events;

/// <summary>
/// Event raised when a diagram is created
/// </summary>
public class DiagramCreatedEvent : DomainEventBase
{
    public DiagramId DiagramId { get; init; }
    public Guid CreatedBy { get; init; }

    public DiagramCreatedEvent(DiagramId diagramId, Guid createdBy)
    {
        DiagramId = diagramId;
        CreatedBy = createdBy;
    }
}

/// <summary>
/// Event raised when an element is added to a diagram
/// </summary>
public class DiagramElementAddedEvent : DomainEventBase
{
    public DiagramId DiagramId { get; init; }
    public ElementId ElementId { get; init; }

    public DiagramElementAddedEvent(DiagramId diagramId, ElementId elementId)
    {
        DiagramId = diagramId;
        ElementId = elementId;
    }
}

/// <summary>
/// Event raised when an element is removed from a diagram
/// </summary>
public class DiagramElementRemovedEvent : DomainEventBase
{
    public DiagramId DiagramId { get; init; }
    public ElementId ElementId { get; init; }

    public DiagramElementRemovedEvent(DiagramId diagramId, ElementId elementId)
    {
        DiagramId = diagramId;
        ElementId = elementId;
    }
}

/// <summary>
/// Event raised when an element is updated
/// </summary>
public class DiagramElementUpdatedEvent : DomainEventBase
{
    public DiagramId DiagramId { get; init; }
    public ElementId ElementId { get; init; }

    public DiagramElementUpdatedEvent(DiagramId diagramId, ElementId elementId)
    {
        DiagramId = diagramId;
        ElementId = elementId;
    }
}

/// <summary>
/// Event raised when a connection is added to a diagram
/// </summary>
public class DiagramConnectionAddedEvent : DomainEventBase
{
    public DiagramId DiagramId { get; init; }
    public ConnectionId ConnectionId { get; init; }

    public DiagramConnectionAddedEvent(DiagramId diagramId, ConnectionId connectionId)
    {
        DiagramId = diagramId;
        ConnectionId = connectionId;
    }
}

/// <summary>
/// Event raised when a connection is removed from a diagram
/// </summary>
public class DiagramConnectionRemovedEvent : DomainEventBase
{
    public DiagramId DiagramId { get; init; }
    public ConnectionId ConnectionId { get; init; }

    public DiagramConnectionRemovedEvent(DiagramId diagramId, ConnectionId connectionId)
    {
        DiagramId = diagramId;
        ConnectionId = connectionId;
    }
}

/// <summary>
/// Event raised when a diagram is exported
/// </summary>
public class DiagramExportedEvent : DomainEventBase
{
    public DiagramId DiagramId { get; init; }
    public string Format { get; init; }
    public Guid ExportedBy { get; init; }

    public DiagramExportedEvent(DiagramId diagramId, string format, Guid exportedBy)
    {
        DiagramId = diagramId;
        Format = format;
        ExportedBy = exportedBy;
    }
}
