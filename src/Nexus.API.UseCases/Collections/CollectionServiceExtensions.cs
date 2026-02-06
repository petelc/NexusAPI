using Microsoft.Extensions.DependencyInjection;
using Nexus.API.UseCases.Collections.Handlers;

namespace Nexus.API.UseCases.Collections;

/// <summary>
/// Extension methods for registering Collection UseCases services
/// </summary>
public static class CollectionServiceExtensions
{
  /// <summary>
  /// Registers all Collection command and query handlers
  /// </summary>
  public static IServiceCollection AddCollectionHandlers(this IServiceCollection services)
  {
    // Command Handlers
    services.AddScoped<CreateCollectionHandler>();
    services.AddScoped<UpdateCollectionHandler>();
    services.AddScoped<DeleteCollectionHandler>();
    services.AddScoped<AddItemToCollectionHandler>();
    services.AddScoped<RemoveItemFromCollectionHandler>();
    services.AddScoped<ReorderItemHandler>();

    // Query Handlers
    services.AddScoped<GetCollectionByIdHandler>();
    services.AddScoped<GetRootCollectionsHandler>();
    services.AddScoped<GetChildCollectionsHandler>();
    services.AddScoped<SearchCollectionsHandler>();
    services.AddScoped<GetCollectionBreadcrumbHandler>();

    return services;
  }
}
