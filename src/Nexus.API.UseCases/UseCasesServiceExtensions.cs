using Microsoft.Extensions.DependencyInjection;

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

    // TODO: Add FluentValidation
    // services.AddValidatorsFromAssembly(typeof(UseCasesServiceExtensions).Assembly);

    return services;
  }
}