using Ardalis.GuardClauses;
using Nexus.API.Core.ValueObjects;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Aggregates.DiagramAggregate;

/// <summary>
/// Layer entity for organizing diagram elements
/// Allows elements to be grouped and managed together
/// </summary>
public class Layer : EntityBase<LayerId>
{
  public string Name { get; private set; } = null!;
  public int Order { get; private set; }
  public bool IsVisible { get; private set; }
  public bool IsLocked { get; private set; }

  private Layer() { } // EF Core

  internal Layer(LayerId id, string name, int order, bool isVisible, bool isLocked)
  {
    Guard.Against.NullOrWhiteSpace(name, nameof(name));
    Guard.Against.Negative(order, nameof(order));

    Id = id;
    Name = name;
    Order = order;
    IsVisible = isVisible;
    IsLocked = isLocked;
  }

  public static Layer CreateDefault()
  {
    return new Layer(
      LayerId.CreateNew(),
      "Default Layer",
      0,
      isVisible: true,
      isLocked: false
    );
  }

  public static Layer Create(string name, int order)
  {
    return new Layer(
      LayerId.CreateNew(),
      name,
      order,
      isVisible: true,
      isLocked: false
    );
  }

  public void Rename(string newName)
  {
    Guard.Against.NullOrWhiteSpace(newName, nameof(newName));
    Name = newName;
  }

  public void UpdateOrder(int newOrder)
  {
    Guard.Against.Negative(newOrder, nameof(newOrder));
    Order = newOrder;
  }

  public void Show()
  {
    IsVisible = true;
  }

  public void Hide()
  {
    IsVisible = false;
  }

  public void Lock()
  {
    IsLocked = true;
  }

  public void Unlock()
  {
    IsLocked = false;
  }
}
