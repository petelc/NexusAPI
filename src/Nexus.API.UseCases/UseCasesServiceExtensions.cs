using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Nexus.API.UseCases.Collections.Validators;
using Nexus.API.UseCases.Collections.Handlers;
using Nexus.API.UseCases.Workspaces.Handlers;
using Nexus.API.UseCases.Teams.Handlers;
using Nexus.API.UseCases.Collaboration.Handlers;

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

    // Collections
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

    // Workspace
    // Command Handlers
    services.AddScoped<CreateWorkspaceHandler>();
    services.AddScoped<UpdateWorkspaceHandler>();
    services.AddScoped<DeleteWorkspaceHandler>();
    services.AddScoped<AddMemberHandler>();
    services.AddScoped<RemoveMemberHandler>();
    services.AddScoped<ChangeMemberRoleHandler>();

    // Query Handlers
    services.AddScoped<GetWorkspaceByIdHandler>();
    services.AddScoped<GetUserWorkspacesHandler>();
    services.AddScoped<GetTeamWorkspacesHandler>();
    services.AddScoped<SearchWorkspacesHandler>();

    // ====================================================================
    // TEAMS HANDLERS
    // ====================================================================
    services.AddScoped<CreateTeamCommandHandler>();
    services.AddScoped<UpdateTeamCommandHandler>();
    services.AddScoped<DeleteTeamCommandHandler>();
    services.AddScoped<AddTeamMemberCommandHandler>();
    services.AddScoped<RemoveTeamMemberCommandHandler>();
    services.AddScoped<ChangeTeamMemberRoleCommandHandler>();
    services.AddScoped<GetTeamByIdQueryHandler>();
    services.AddScoped<GetUserTeamsQueryHandler>();
    services.AddScoped<SearchTeamsQueryHandler>();

    // ====================================================================
    // WORKSPACES HANDLERS
    // ====================================================================
    services.AddScoped<CreateWorkspaceHandler>();
    services.AddScoped<UpdateWorkspaceHandler>();
    services.AddScoped<DeleteWorkspaceHandler>();
    services.AddScoped<AddMemberHandler>();
    services.AddScoped<RemoveMemberHandler>();
    services.AddScoped<ChangeMemberRoleHandler>();
    services.AddScoped<GetWorkspaceByIdHandler>();
    services.AddScoped<GetUserWorkspacesHandler>();
    services.AddScoped<GetTeamWorkspacesHandler>();
    services.AddScoped<SearchWorkspacesHandler>();

    // ====================================================================
    // COLLABORATION HANDLERS
    // ====================================================================
    services.AddScoped<StartSessionCommandHandler>();
    services.AddScoped<EndSessionCommandHandler>();
    services.AddScoped<JoinSessionCommandHandler>();
    services.AddScoped<LeaveSessionCommandHandler>();
    services.AddScoped<GetSessionByIdQueryHandler>();
    services.AddScoped<GetActiveSessionsQueryHandler>();
    services.AddScoped<GetUserSessionsQueryHandler>();


    return services;
  }
}