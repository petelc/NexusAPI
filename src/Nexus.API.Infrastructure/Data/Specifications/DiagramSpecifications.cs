using Ardalis.Specification;
using Nexus.API.Core.Aggregates.DiagramAggregate;
using Nexus.API.Core.Enums;

namespace Nexus.API.Infrastructure.Data.Specifications;

/// <summary>
/// Specification for getting diagrams by user with all related entities
/// </summary>
public class DiagramsByUserSpec : Specification<Diagram>
{
  public DiagramsByUserSpec(Guid userId)
  {
    Query
      .Where(d => d.CreatedBy == userId)
      .Include("_elements")
      .Include("_connections")
      .Include("_layers")
      .OrderByDescending(d => d.UpdatedAt);
  }
}

/// <summary>
/// Specification for getting diagrams by type
/// </summary>
public class DiagramsByTypeSpec : Specification<Diagram>
{
  public DiagramsByTypeSpec(DiagramType diagramType)
  {
    Query
      .Where(d => d.DiagramType == diagramType)
      .OrderByDescending(d => d.UpdatedAt);
  }
}

/// <summary>
/// Specification for getting a single diagram with all related entities
/// </summary>
public class DiagramByIdWithRelatedSpec : Specification<Diagram>
{
  public DiagramByIdWithRelatedSpec(Guid diagramId)
  {
    Query
      .Where(d => d.Id.Value == diagramId)
      .Include("_elements")
      .Include("_connections")
      .Include("_layers");
  }
}

/// <summary>
/// Specification for searching diagrams by title
/// </summary>
public class DiagramSearchSpec : Specification<Diagram>
{
  public DiagramSearchSpec(string searchTerm)
  {
    Query
      .Where(d => d.Title.Value.Contains(searchTerm))
      .OrderByDescending(d => d.UpdatedAt);
  }
}

/// <summary>
/// Specification for getting user's diagrams by type
/// </summary>
public class DiagramsByUserAndTypeSpec : Specification<Diagram>
{
  public DiagramsByUserAndTypeSpec(Guid userId, DiagramType diagramType)
  {
    Query
      .Where(d => d.CreatedBy == userId && d.DiagramType == diagramType)
      .Include("_elements")
      .Include("_connections")
      .Include("_layers")
      .OrderByDescending(d => d.UpdatedAt);
  }
}

/// <summary>
/// Specification for paginated diagrams with optional filters
/// </summary>
public class PaginatedDiagramsSpec : Specification<Diagram>
{
  public PaginatedDiagramsSpec(
    int page,
    int pageSize,
    Guid? userId = null,
    DiagramType? diagramType = null)
  {
    if (userId.HasValue)
    {
      Query.Where(d => d.CreatedBy == userId.Value);
    }

    if (diagramType.HasValue)
    {
      Query.Where(d => d.DiagramType == diagramType.Value);
    }

    Query
      .OrderByDescending(d => d.UpdatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize);
  }
}

/// <summary>
/// Specification for counting user's diagrams
/// </summary>
public class CountDiagramsByUserSpec : Specification<Diagram>
{
  public CountDiagramsByUserSpec(Guid userId)
  {
    Query.Where(d => d.CreatedBy == userId);
  }
}
