using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Traxs.SharedKernel;

namespace Nexus.API.Infrastructure.Data;

/// <summary>
/// Generic repository base implementing IRepositoryBase from Traxs.SharedKernel
/// Uses Ardalis.Specification for query specifications
/// </summary>
public class RepositoryBase<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
  protected readonly AppDbContext _dbContext;
  private readonly ISpecificationEvaluator _specificationEvaluator;

  public RepositoryBase(AppDbContext dbContext)
  {
    _dbContext = dbContext;
    _specificationEvaluator = SpecificationEvaluator.Default;
  }

  public RepositoryBase(AppDbContext dbContext, ISpecificationEvaluator specificationEvaluator)
  {
    _dbContext = dbContext;
    _specificationEvaluator = specificationEvaluator;
  }

  // Write operations
  public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
  {
    await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
    await SaveChangesAsync(cancellationToken);
    return entity;
  }

  public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
  {
    var entityList = entities.ToList();
    await _dbContext.Set<T>().AddRangeAsync(entityList, cancellationToken);
    await SaveChangesAsync(cancellationToken);
    return entityList;
  }

  public async Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default)
  {
    // Only call Update() for detached entities. For already-tracked entities,
    // the change tracker already knows what changed — calling Update() would
    // incorrectly mark newly Added child entities as Modified.
    var entry = _dbContext.Entry(entity);
    if (entry.State == EntityState.Detached)
    {
      _dbContext.Set<T>().Update(entity);
    }
    return await SaveChangesAsync(cancellationToken);
  }

  public async Task<int> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
  {
    _dbContext.Set<T>().UpdateRange(entities);
    return await SaveChangesAsync(cancellationToken);
  }

  public async Task<int> DeleteAsync(T entity, CancellationToken cancellationToken = default)
  {
    _dbContext.Set<T>().Remove(entity);
    return await SaveChangesAsync(cancellationToken);
  }

  public async Task<int> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
  {
    _dbContext.Set<T>().RemoveRange(entities);
    return await SaveChangesAsync(cancellationToken);
  }

  public async Task<int> DeleteRangeAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
  {
    var entities = await ListAsync(specification, cancellationToken);
    _dbContext.Set<T>().RemoveRange(entities);
    return await SaveChangesAsync(cancellationToken);
  }

  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.SaveChangesAsync(cancellationToken);
  }

  // Read operations
  public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
  {
    return await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
  }

  public async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
  }

  public async Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
  }

  public async Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<T>().ToListAsync(cancellationToken);
  }

  public async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification).ToListAsync(cancellationToken);
  }

  public async Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification).ToListAsync(cancellationToken);
  }

  public async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification).CountAsync(cancellationToken);
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<T>().CountAsync(cancellationToken);
  }

  public async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification).AnyAsync(cancellationToken);
  }

  public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<T>().AnyAsync(cancellationToken);
  }

  public IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> specification)
  {
    return ApplySpecification(specification).AsAsyncEnumerable();
  }

  // Specification helpers
  private IQueryable<T> ApplySpecification(ISpecification<T> specification)
  {
    return _specificationEvaluator.GetQuery(_dbContext.Set<T>().AsQueryable(), specification);
  }

  private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification)
  {
    return _specificationEvaluator.GetQuery(_dbContext.Set<T>().AsQueryable(), specification);
  }
}

/// <summary>
/// Read-only repository base
/// </summary>
public class ReadRepositoryBase<T> : RepositoryBase<T>, IReadRepositoryBase<T>
  where T : class, IAggregateRoot
{
  public ReadRepositoryBase(AppDbContext dbContext) : base(dbContext)
  {
  }

  public ReadRepositoryBase(AppDbContext dbContext, ISpecificationEvaluator specificationEvaluator)
    : base(dbContext, specificationEvaluator)
  {
  }
}
