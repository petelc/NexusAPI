using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Data;
using Traxs.SharedKernel;

namespace Nexus.API.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User aggregate
/// </summary>
public class UserRepository : IUserRepository
{
  private readonly AppDbContext _dbContext;

  public UserRepository(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users
      .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
  }

  public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users
      .FirstOrDefaultAsync(u => u.Email.Address == email.ToLowerInvariant(), cancellationToken);
  }

  public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users
      .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
  }

  public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users
      .AnyAsync(u => u.Email.Address == email.ToLowerInvariant(), cancellationToken);
  }

  public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users
      .AnyAsync(u => u.Username == username, cancellationToken);
  }

  public async Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users
      .Where(u => u.IsActive)
      .ToListAsync(cancellationToken);
  }

  // IRepositoryBase implementation
  public async Task<User?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
  {
    if (id is UserId userId)
      return await GetByIdAsync(userId, cancellationToken);

    return null;
  }

  public async Task<User?> FirstOrDefaultAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification)
      .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<User, TResult> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification)
      .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<User?> SingleOrDefaultAsync(ISingleResultSpecification<User> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification)
      .SingleOrDefaultAsync(cancellationToken);
  }

  public async Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<User, TResult> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification)
      .SingleOrDefaultAsync(cancellationToken);
  }

  public async Task<List<User>> ListAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users.ToListAsync(cancellationToken);
  }

  public async Task<List<User>> ListAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification)
      .ToListAsync(cancellationToken);
  }

  public async Task<List<TResult>> ListAsync<TResult>(ISpecification<User, TResult> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification)
      .ToListAsync(cancellationToken);
  }

  public IAsyncEnumerable<User> AsAsyncEnumerable(ISpecification<User> specification)
  {
    return ApplySpecification(specification).AsAsyncEnumerable();
  }

  public async Task<int> CountAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification)
      .CountAsync(cancellationToken);
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users.CountAsync(cancellationToken);
  }

  public async Task<bool> AnyAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
  {
    return await ApplySpecification(specification)
      .AnyAsync(cancellationToken);
  }

  public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users.AnyAsync(cancellationToken);
  }

  public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
  {
    await _dbContext.Users.AddAsync(entity, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
    return entity;
  }

  public async Task<IEnumerable<User>> AddRangeAsync(IEnumerable<User> entities, CancellationToken cancellationToken = default)
  {
    var entityList = entities.ToList();
    await _dbContext.Users.AddRangeAsync(entityList, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
    return entityList;
  }

  public async Task<int> UpdateAsync(User entity, CancellationToken cancellationToken = default)
  {
    _dbContext.Users.Update(entity);
    return await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task<int> UpdateRangeAsync(IEnumerable<User> entities, CancellationToken cancellationToken = default)
  {
    _dbContext.Users.UpdateRange(entities);
    return await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task<int> DeleteAsync(User entity, CancellationToken cancellationToken = default)
  {
    _dbContext.Users.Remove(entity);
    return await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task<int> DeleteRangeAsync(IEnumerable<User> entities, CancellationToken cancellationToken = default)
  {
    _dbContext.Users.RemoveRange(entities);
    return await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task<int> DeleteRangeAsync(ISpecification<User> specification, CancellationToken cancellationToken = default)
  {
    var entities = await ListAsync(specification, cancellationToken);
    _dbContext.Users.RemoveRange(entities);
    return await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.SaveChangesAsync(cancellationToken);
  }

  // Helper method to apply specifications
  private IQueryable<User> ApplySpecification(ISpecification<User> specification)
  {
    var evaluator = new SpecificationEvaluator();
    return evaluator.GetQuery(_dbContext.Users.AsQueryable(), specification);
  }

  private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<User, TResult> specification)
  {
    var evaluator = new SpecificationEvaluator();
    return evaluator.GetQuery(_dbContext.Users.AsQueryable(), specification);
  }
}
