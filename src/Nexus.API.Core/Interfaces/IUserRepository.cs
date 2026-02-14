using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.ValueObjects;
using Traxs.SharedKernel;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Repository interface for User aggregate
/// </summary>
public interface IUserRepository : IRepositoryBase<User>
{
  /// <summary>
  /// Get a user by their ID
  /// </summary>
  Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get a user by their email address
  /// </summary>
  Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get a user by their username
  /// </summary>
  Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if an email address is already in use
  /// </summary>
  Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if a username is already in use
  /// </summary>
  Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get all active users
  /// </summary>
  Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

}

/// <summary>
/// DTO representing a user for the UseCases layer.
/// </summary>
public class UserDto
{
  public string Id { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
}

