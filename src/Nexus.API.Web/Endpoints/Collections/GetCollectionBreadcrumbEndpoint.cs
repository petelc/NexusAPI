using FastEndpoints;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.UseCases.Collections.DTOs;
using Nexus.API.UseCases.Collections.Queries;

namespace Nexus.API.Web.Endpoints.Collections;

/// <summary>
/// Endpoint: GET /api/v1/collections/{id}/breadcrumb
/// Gets collection breadcrumb (ancestor chain)
/// Requires: Viewer, Editor, Admin roles
/// </summary>
public class GetCollectionBreadcrumbEndpoint : EndpointWithoutRequest
{
  private readonly ICollectionRepository _collectionRepository;

  public GetCollectionBreadcrumbEndpoint(ICollectionRepository collectionRepository)
  {
    _collectionRepository = collectionRepository;
  }

  public override void Configure()
  {
    Get("/collections/{id}/breadcrumb");
    Roles("Viewer", "Editor", "Admin");

    Description(b => b
      .WithTags("Collections")
      .WithSummary("Get collection breadcrumb")
      .WithDescription("Retrieves the full ancestor chain from root to current collection"));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(Route<string>("id"), out var collectionId) || collectionId == Guid.Empty)
    {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsJsonAsync(new { error = "Invalid collection ID" }, ct);
      return;
    }

    try
    {
      var collectionIdValue = CollectionId.Create(collectionId);
      var hierarchy = await _collectionRepository.GetHierarchyAsync(collectionIdValue, ct);

      var breadcrumb = hierarchy.Select(MapToSummaryDto).ToList();
      var response = new GetCollectionBreadcrumbResponse { Breadcrumb = breadcrumb };

      HttpContext.Response.StatusCode = 200;
      await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
    catch (Exception ex)
    {
      HttpContext.Response.StatusCode = 500;
      await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
    }
  }

  private static CollectionSummaryDto MapToSummaryDto(Collection collection)
  {
    return new CollectionSummaryDto
    {
      CollectionId = collection.Id.Value,
      Name = collection.Name,
      Icon = collection.Icon,
      Color = collection.Color,
      ParentCollectionId = collection.ParentCollectionId?.Value,
      HierarchyLevel = collection.HierarchyPath.Level,
      ItemCount = collection.GetItemCount(),
      UpdatedAt = collection.UpdatedAt
    };
  }
}
