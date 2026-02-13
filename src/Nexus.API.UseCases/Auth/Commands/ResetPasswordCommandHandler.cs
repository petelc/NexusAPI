using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.UseCases.Auth.Commands;

/// <summary>
/// Command to reset a password using a valid Identity token.
/// </summary>
public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword) : IRequest<Result<ResetPasswordResponse>>;

/// <summary>
/// Handles the "Reset Password" request.
/// Validates inputs and delegates password reset to ASP.NET Identity via IUserService.
///
/// Security Features:
/// - Token validation handled by Identity's token provider
/// - Password requirements enforced by Identity
/// - Single-use tokens enforced by Identity
/// </summary>
public class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
{
    private readonly IUserService _userService;

    public ResetPasswordCommandHandler(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result<ResetPasswordResponse>> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var validationErrors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = nameof(command.Email),
                ErrorMessage = "Email is required"
            });
        }

        if (string.IsNullOrWhiteSpace(command.Token))
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = nameof(command.Token),
                ErrorMessage = "Reset token is required"
            });
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword))
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = nameof(command.NewPassword),
                ErrorMessage = "New password is required"
            });
        }

        if (command.NewPassword != command.ConfirmPassword)
        {
            validationErrors.Add(new ValidationError
            {
                Identifier = nameof(command.ConfirmPassword),
                ErrorMessage = "Passwords do not match"
            });
        }

        if (validationErrors.Any())
        {
            return Result.Invalid(validationErrors);
        }

        var user = await _userService.FindByEmailAsync(command.Email, cancellationToken);

        if (user == null)
        {
            return Result.Error("Invalid or expired reset token");
        }

        var resetSuccess = await _userService.ResetPasswordAsync(
            user.Id,
            command.Token,
            command.NewPassword,
            cancellationToken);

        if (!resetSuccess)
        {
            return Result.Error("Failed to reset password. The token may be invalid or expired, or the new password does not meet requirements.");
        }

        return Result.Success(new ResetPasswordResponse
        {
            Message = "Password has been reset successfully"
        });
    }
}
