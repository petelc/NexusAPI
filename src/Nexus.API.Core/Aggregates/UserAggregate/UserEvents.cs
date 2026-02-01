using Traxs.SharedKernel;
using Nexus.API.Core.ValueObjects;

namespace Nexus.API.Core.Aggregates.UserAggregate;

/// <summary>
/// Event raised when a new user is created
/// </summary>
public class UserCreatedEvent : DomainEventBase
{
    public UserId UserId { get; init; }
    public Email Email { get; init; } = null!;
    public string Username { get; init; } = null!;

    public UserCreatedEvent(UserId userId, Email email, string username)
    {
        UserId = userId;
        Email = email;
        Username = username;
    }
}

/// <summary>
/// Event raised when a user's email is confirmed
/// </summary>
public class UserEmailConfirmedEvent : DomainEventBase
{
    public UserId UserId { get; init; }
    public Email Email { get; init; } = null!;

    public UserEmailConfirmedEvent(UserId userId, Email email)
    {
        UserId = userId;
        Email = email;
    }
}

/// <summary>
/// Event raised when two-factor authentication is enabled
/// </summary>
public class UserTwoFactorEnabledEvent : DomainEventBase
{
    public UserId UserId { get; init; }

    public UserTwoFactorEnabledEvent(UserId userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Event raised when two-factor authentication is disabled
/// </summary>
public class UserTwoFactorDisabledEvent : DomainEventBase
{
    public UserId UserId { get; init; }

    public UserTwoFactorDisabledEvent(UserId userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Event raised when a user successfully logs in
/// </summary>
public class UserLoggedInEvent : DomainEventBase
{
    public UserId UserId { get; init; }
    public DateTime LoginTime { get; init; }

    public UserLoggedInEvent(UserId userId, DateTime loginTime)
    {
        UserId = userId;
        LoginTime = loginTime;
    }
}

/// <summary>
/// Event raised when a user's password is changed
/// </summary>
public class UserPasswordChangedEvent : DomainEventBase
{
    public UserId UserId { get; init; }

    public UserPasswordChangedEvent(UserId userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Event raised when a user account is deactivated
/// </summary>
public class UserDeactivatedEvent : DomainEventBase
{
    public UserId UserId { get; init; }

    public UserDeactivatedEvent(UserId userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// Event raised when a user account is reactivated
/// </summary>
public class UserReactivatedEvent : DomainEventBase
{
    public UserId UserId { get; init; }

    public UserReactivatedEvent(UserId userId)
    {
        UserId = userId;
    }
}
