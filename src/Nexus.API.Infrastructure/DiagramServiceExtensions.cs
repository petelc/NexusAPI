// Add to your Infrastructure/ServiceCollectionExtensions.cs or similar DI setup file

using Microsoft.Extensions.DependencyInjection;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Data.Repositories;

namespace Nexus.API.Infrastructure;

public static class DiagramServiceExtensions
{
  public static IServiceCollection AddDiagramInfrastructure(this IServiceCollection services)
  {
    // Register Diagram Repository
    services.AddScoped<IDiagramRepository, DiagramRepository>();

    return services;
  }
}

// OR if you have a single registration method, add this line:
// services.AddScoped<IDiagramRepository, DiagramRepository>();
