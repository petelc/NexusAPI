using Ardalis.Result;
using Nexus.API.Core.Aggregates.CollectionAggregate;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.UseCases.Collections.Commands;
using Nexus.API.UseCases.Collections.DTOs;

namespace Nexus.API.UseCases.Collections.Handlers;

/// <summary>
/// Handler for creating a new collection
/// </summary>
public class CreateCollectionHandler
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
    if (userId == Guid.Empty)
    {
      return Result<CreateCollectionResponse>.Unauthorized();
    }

    // Check for duplicate name in same parent
    var nameExists = await _collectionRepository.ExistsWithNameAsync(
      WorkspaceId.Create(command.WorkspaceId),
      command.Name,
      command.ParentCollectionId.HasValue
        ? CollectionId.Create(command.ParentCollectionId.Value)
        : null,
      null,
      cancellationToken);

    if (nameExists)
    {
      return Result<CreateCollectionResponse>.Error(
        "A collection with this name already exists in the same location");
    }

    // Create collection
    Collection collection;

    if (command.ParentCollectionId.HasValue)
    {
      // Child collection
      var parentId = CollectionId.Create(command.ParentCollectionId.Value);
      var parent = await _collectionRepository.GetByIdAsync(parentId, cancellationToken);

      if (parent == null)
      {
        return Result<CreateCollectionResponse>.NotFound("Parent collection not found");
      }

      collection = Collection.CreateChild(
        command.Name,
        WorkspaceId.Create(command.WorkspaceId),
        userId,
        parentId,
        parent.HierarchyPath,
        command.Description,
        command.Icon,
        command.Color);
    }
    else
    {
      // Root collection
      collection = Collection.CreateRoot(
        command.Name,
        WorkspaceId.Create(command.WorkspaceId),
        userId,
        command.Description,
        command.Icon,
        command.Color);
    }

    await _collectionRepository.AddAsync(collection, cancellationToken);

    var dto = MapToDto(collection);

    return Result<CreateCollectionResponse>.Success(
      new CreateCollectionResponse { Collection = dto });
  }

  private static CollectionDto MapToDto(Collection collection)
  {
    return new CollectionDto
    {
      CollectionId = collection.Id.Value,
      Name = collection.Name,
      Description = collection.Description,
      ParentCollectionId = collection.ParentCollectionId?.Value,
      WorkspaceId = collection.WorkspaceId.Value,
      CreatedBy = collection.CreatedBy,
      CreatedAt = collection.CreatedAt,
      UpdatedAt = collection.UpdatedAt,
      Icon = collection.Icon,
      Color = collection.Color,
      OrderIndex = collection.OrderIndex,
      HierarchyLevel = collection.HierarchyPath.Level,
      HierarchyPath = collection.HierarchyPath.Value,
      ItemCount = collection.GetItemCount(),
      Items = collection.Items.Select(item => new CollectionItemDto
      {
        CollectionItemId = item.Id.Value,
        ItemType = item.ItemType.ToString(),
        ItemReferenceId = item.ItemReferenceId,
        Order = item.Order,
        AddedBy = item.AddedBy,
        AddedAt = item.AddedAt
      }).ToList()
    };
  }
}
