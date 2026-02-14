namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Service for user management operations.
/// Abstracts ASP.NET Core Identity from the UseCases layer.
/// Implementation should be in Infrastructure layer.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Finds a user by email address.
    /// </summary>
    Task<UserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a password reset token for the specified user using ASP.NET Identity.
    /// </summary>
    Task<string?> GeneratePasswordResetTokenAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password using a valid Identity token.
    /// </summary>
    Task<bool> ResetPasswordAsync(
        string userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);
}


