using Ardalis.GuardClauses;
using Traxs.SharedKernel;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Core.Interfaces;

namespace Nexus.API.Core.Aggregates.UserAggregate;

/// <summary>
/// User aggregate root - represents a user in the knowledge management system
/// </summary>
public class User : EntityBase<UserId>, IAggregateRoot
{
  public Email Email { get; private set; }
  public string Username { get; private set; }
  public PersonName FullName { get; private set; }
  public string PasswordHash { get; private set; }
  public bool EmailConfirmed { get; private set; }
  public bool TwoFactorEnabled { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime? LastLoginAt { get; private set; }
  public UserProfile Profile { get; private set; }
  public UserPreferences Preferences { get; private set; }
  public bool IsActive { get; private set; }

  // Private constructor for EF Core
  private User()
  {
    Email = null!;
    Username = null!;
    FullName = null!;
    PasswordHash = null!;
    Profile = null!;
    Preferences = null!;
  }

  private User(
    UserId id,
    Email email,
    string username,
    PersonName fullName,
    string passwordHash)
  {
    Id = id;
    Email = email;
    Username = username;
    FullName = fullName;
    PasswordHash = passwordHash;
    EmailConfirmed = false;
    TwoFactorEnabled = false;
    CreatedAt = DateTime.UtcNow;
    Profile = UserProfile.Empty;
    Preferences = UserPreferences.Default;
    IsActive = true;
  }

  /// <summary>
  /// Factory method to create a new user
  /// </summary>
  public static User Create(
    Email email,
    string username,
    PersonName fullName,
    string passwordHash)
  {
    Guard.Against.Null(email, nameof(email));
    Guard.Against.NullOrWhiteSpace(username, nameof(username));
    Guard.Against.Null(fullName, nameof(fullName));
    Guard.Against.NullOrWhiteSpace(passwordHash, nameof(passwordHash));

    var user = new User(
      UserId.CreateNew(),
      email,
      username,
      fullName,
      passwordHash
    );

    // Raise domain event
    user.RegisterDomainEvent(new UserCreatedEvent(user.Id, email, username));

    return user;
  }

  /// <summary>
  /// Factory method to create a domain user from an existing identity user (same ID)
  /// </summary>
  public static User CreateFromIdentity(
    Guid existingId,
    Email email,
    string username,
    PersonName fullName,
    string passwordHash)
  {
    Guard.Against.Null(email, nameof(email));
    Guard.Against.NullOrWhiteSpace(username, nameof(username));
    Guard.Against.Null(fullName, nameof(fullName));
    Guard.Against.NullOrWhiteSpace(passwordHash, nameof(passwordHash));

    var user = new User(
      UserId.From(existingId),
      email,
      username,
      fullName,
      passwordHash
    );

    user.RegisterDomainEvent(new UserCreatedEvent(user.Id, email, username));

    return user;
  }

  /// <summary>
  /// Confirm the user's email address
  /// </summary>
  public void ConfirmEmail()
  {
    if (EmailConfirmed)
      return;

    EmailConfirmed = true;
    RegisterDomainEvent(new UserEmailConfirmedEvent(Id, Email));
  }

  /// <summary>
  /// Enable two-factor authentication
  /// </summary>
  public void EnableTwoFactor()
  {
    if (TwoFactorEnabled)
      return;

    TwoFactorEnabled = true;
    RegisterDomainEvent(new UserTwoFactorEnabledEvent(Id));
  }

  /// <summary>
  /// Disable two-factor authentication
  /// </summary>
  public void DisableTwoFactor()
  {
    if (!TwoFactorEnabled)
      return;

    TwoFactorEnabled = false;
    RegisterDomainEvent(new UserTwoFactorDisabledEvent(Id));
  }

  /// <summary>
  /// Record a successful login
  /// </summary>
  public void RecordLogin()
  {
    LastLoginAt = DateTime.UtcNow;
    RegisterDomainEvent(new UserLoggedInEvent(Id, LastLoginAt.Value));
  }

  /// <summary>
  /// Update the user's profile
  /// </summary>
  public void UpdateProfile(UserProfile newProfile)
  {
    Guard.Against.Null(newProfile, nameof(newProfile));
    Profile = newProfile;
  }

  /// <summary>
  /// Update the user's preferences
  /// </summary>
  public void UpdatePreferences(UserPreferences newPreferences)
  {
    Guard.Against.Null(newPreferences, nameof(newPreferences));
    Preferences = newPreferences;
  }

  /// <summary>
  /// Change the user's password
  /// </summary>
  public void ChangePassword(string newPasswordHash)
  {
    Guard.Against.NullOrWhiteSpace(newPasswordHash, nameof(newPasswordHash));
    PasswordHash = newPasswordHash;
    RegisterDomainEvent(new UserPasswordChangedEvent(Id));
  }

  /// <summary>
  /// Update the user's full name
  /// </summary>
  public void UpdateFullName(PersonName newFullName)
  {
    Guard.Against.Null(newFullName, nameof(newFullName));
    FullName = newFullName;
  }

  /// <summary>
  /// Deactivate the user account
  /// </summary>
  public void Deactivate()
  {
    if (!IsActive)
      return;

    IsActive = false;
    RegisterDomainEvent(new UserDeactivatedEvent(Id));
  }

  /// <summary>
  /// Reactivate the user account
  /// </summary>
  public void Reactivate()
  {
    if (IsActive)
      return;

    IsActive = true;
    RegisterDomainEvent(new UserReactivatedEvent(Id));
  }
}
