using Nexus.API.UseCases.Collaboration.Interfaces;
using Nexus.API.Web.Services;

namespace Nexus.API.Web.Extensions;

/// <summary>
/// Extension method to register collaboration real-time services.
/// Call this from Program.cs AFTER builder.Services.AddSignalR().
///
/// Dependency direction is correct:
///   UseCases defines ICollaborationNotificationService
///   Web implements SignalRCollaborationNotificationService
///   UseCases handlers only depend on the interface
/// </summary>
public static class CollaborationServicesExtensions
{
    public static IServiceCollection AddCollaborationServices(
        this IServiceCollection services)
    {
        // The SignalR implementation lives in Web and is registered here.
        // UseCases handlers receive ICollaborationNotificationService via DI
        // and never know about SignalR or the hub.
        services.AddScoped<ICollaborationNotificationService,
                           SignalRCollaborationNotificationService>();

        return services;
    }
}
