using Ardalis.Result;
using MediatR;
using Nexus.API.Core.Interfaces;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.UseCases.Auth.Commands;

/// <summary>
/// Command to initiate the forgot password flow.
/// </summary>
public record ForgotPasswordCommand(string Email) : IRequest<Result<ForgotPasswordResponse>>;

/// <summary>
/// Handles the "Forgot Password" request.
/// Uses ASP.NET Identity's built-in token provider to generate a reset token.
///
/// Security Features:
/// - Always returns success to prevent email enumeration
/// - Token generation delegated to Identity's DefaultTokenProviders
/// - Tokens are time-limited by Identity configuration
/// </summary>
public class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IUserService userService,
        IEmailService emailService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public async Task<Result<ForgotPasswordResponse>> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Invalid(new ValidationError
            {
                Identifier = nameof(command.Email),
                ErrorMessage = "Email is required"
            });
        }

        var user = await _userService.FindByEmailAsync(command.Email, cancellationToken);

        // Always return success to prevent email enumeration
        if (user == null)
        {
            return Result.Success(new ForgotPasswordResponse
            {
                Message = "If an account exists with this email, a password reset link has been sent."
            });
        }

        var token = await _userService.GeneratePasswordResetTokenAsync(user.Id, cancellationToken);

        if (token != null)
        {
            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                user.UserName,
                token,
                cancellationToken);
        }

        return Result.Success(new ForgotPasswordResponse
        {
            Message = "If an account exists with this email, a password reset link has been sent."
        });
    }
}
