using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Nexus.API.UseCases.Collections.Validators;
using Nexus.API.UseCases.Collections.Handlers;
using Nexus.API.UseCases.Workspaces.Handlers;
using Nexus.API.UseCases.Teams.Handlers;
using Nexus.API.UseCases.Collaboration.Handlers;
using Nexus.API.UseCases.Permissions.Commands;
using Nexus.API.UseCases.Permissions.Queries;
using Nexus.API.UseCases.Documents.Commands.UpdateDocument;
using Nexus.API.UseCases.Documents.Commands;
using Nexus.API.UseCases.Documents.Queries;
using Nexus.API.UseCases.Search.Queries;
namespace Nexus.API.UseCases;

/// <summary>
/// Extension methods for registering UseCases layer services
/// </summary>
public static class UseCasesServiceExtensions
{
  public static IServiceCollection AddUseCasesServices(
    this IServiceCollection services)
  {
    // Register MediatR handlers from this assembly
    services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssembly(typeof(UseCasesServiceExtensions).Assembly));

    // Register AutoMapper
    services.AddAutoMapper(typeof(UseCasesServiceExtensions).Assembly);

    services.AddScoped<GlobalSearchQueryHandler>();

    return services;
  }
}