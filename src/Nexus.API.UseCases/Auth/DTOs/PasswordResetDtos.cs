namespace Nexus.API.UseCases.Auth.DTOs;

/// <summary>
/// Request to initiate the forgot password flow.
/// </summary>
public record ForgotPasswordRequest
{
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Response for forgot password — intentionally vague for security.
/// </summary>
public record ForgotPasswordResponse
{
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Request to reset password with a token.
/// </summary>
public record ResetPasswordRequest
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
}

/// <summary>
/// Response for successful password reset.
/// </summary>
public record ResetPasswordResponse
{
    public string Message { get; init; } = string.Empty;
}
