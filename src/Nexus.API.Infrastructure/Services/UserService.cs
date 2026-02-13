using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;

namespace Nexus.API.Infrastructure.Services;

/// <summary>
/// Implementation of IUserService using ASP.NET Core Identity.
/// This keeps Identity dependencies in the Infrastructure layer.
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<UserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty
        };
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return null;

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(
        string userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
            return false;

        // Use Identity's built-in password reset (it validates the token format)
        // Note: We're using the plaintext token here, not the hash
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        return result.Succeeded;
    }
}
