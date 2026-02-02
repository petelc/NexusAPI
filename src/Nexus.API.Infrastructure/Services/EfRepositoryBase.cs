using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nexus.API.Infrastructure.Data;

namespace Nexus.API.Infrastructure.Services;

/// <summary>
/// EF Core repository implementation with Ardalis.Specification support
/// Implements both IRepositoryBase and IReadRepositoryBase from Traxs.SharedKernel
/// Uses Ardalis.Specification for advanced querying
/// </summary>
public class EfRepositoryBase<T> : Ardalis.Specification.EntityFrameworkCore.RepositoryBase<T>, IRepositoryBase<T>, IReadRepositoryBase<T>
  where T : class, IAggregateRoot
{
  private readonly AppDbContext _dbContext;
  private readonly IDomainEventDispatcher _dispatcher;

  public EfRepositoryBase(AppDbContext dbContext, IDomainEventDispatcher dispatcher)
    : base(dbContext)
  {
    _dbContext = dbContext;
    _dispatcher = dispatcher;
  }

  // Override SaveChangesAsync to dispatch domain events
  public override async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
  {
    _dbContext.Set<T>().Add(entity);
    await SaveChangesAsync(cancellationToken);
    return entity;
  }

  public override async Task<IEnumerable<T>> AddRangeAsync(
    IEnumerable<T> entities,
    CancellationToken cancellationToken = default)
  {
    _dbContext.Set<T>().AddRange(entities);
    await SaveChangesAsync(cancellationToken);
    return entities;
  }

  public override async Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default)
  {
    _dbContext.Set<T>().Update(entity);
    return await SaveChangesAsync(cancellationToken);
  }

  public override async Task<int> UpdateRangeAsync(
    IEnumerable<T> entities,
    CancellationToken cancellationToken = default)
  {
    _dbContext.Set<T>().UpdateRange(entities);
    return await SaveChangesAsync(cancellationToken);
  }

  public override async Task<int> DeleteAsync(T entity, CancellationToken cancellationToken = default)
  {
    _dbContext.Set<T>().Remove(entity);
    return await SaveChangesAsync(cancellationToken);
  }

  public override async Task<int> DeleteRangeAsync(
    IEnumerable<T> entities,
    CancellationToken cancellationToken = default)
  {
    _dbContext.Set<T>().RemoveRange(entities);
    return await SaveChangesAsync(cancellationToken);
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    // Dispatch domain events before saving
    var entitiesWithEvents = _dbContext.ChangeTracker
      .Entries<EntityBase>()
      .Select(e => e.Entity)
      .Where(e => e.DomainEvents.Any())
      .ToArray();

    var result = await _dbContext.SaveChangesAsync(cancellationToken);

    // Dispatch events after successful save
    await _dispatcher.DispatchAndClearEvents(entitiesWithEvents);

    return result;
  }
}
