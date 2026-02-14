using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Aggregates.WorkspaceAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.Commands;
using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Handlers;

/// <summary>
/// Handler for creating a new collection
/// </summary>
public class CreateCollectionHandler : IRequestHandler<CreateCollectionCommand, Result<CreateCollectionResponse>>
{
  private readonly ICollectionRepository _collectionRepository;
  private readonly ICurrentUserService _currentUserService;

  public CreateCollectionHandler(
    ICollectionRepository collectionRepository,
    ICurrentUserService currentUserService)
  {
    _collectionRepository = collectionRepository;
    _currentUserService = currentUserService;
  }

  public async Task<Result<CreateCollectionResponse>> Handle(
    CreateCollectionCommand command,
    CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredUserId();
    var workspaceId = WorkspaceId.From(command.WorkspaceId.ToString());

    Collection collection;

    if (command.ParentCollectionId.HasValue)
    {
      // Creating a child collection
      var parentId = CollectionId.From(command.ParentCollectionId.Value.ToString());
      var parent = await _collectionRepository.GetByIdAsync(parentId, cancellationToken);

      if (parent == null)
      {
        return Result<CreateCollectionResponse>.NotFound("Parent collection not found");
      }

      collection = Collection.CreateChild(
        command.Name,
        workspaceId,
        userId,
        parentId,
        parent.HierarchyPath,
        command.Description,
        command.Icon,
        command.Color);
    }
    else
    {
      // Creating a root collection
      collection = Collection.CreateRoot(
        command.Name,
        workspaceId,
        userId,
        command.Description,
        command.Icon,
        command.Color);
    }

    await _collectionRepository.AddAsync(collection, cancellationToken);

    var dto = collection.ToDto();

    return Result<CreateCollectionResponse>.Success(
      new CreateCollectionResponse { Collection = dto });
  }
}