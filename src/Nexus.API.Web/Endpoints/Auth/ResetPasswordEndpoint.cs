using FastEndpoints;
using MediatR;
using Nexus.API.UseCases.Auth.Commands;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.Web.Endpoints.Auth;

/// <summary>
/// Resets a password using a valid token.
/// POST /api/v1/auth/reset-password
///
/// The token comes from the forgot password email link.
/// </summary>
public class ResetPasswordEndpoint : Endpoint<ResetPasswordRequest, ResetPasswordResponse>
{
    private readonly IMediator _mediator;

    public ResetPasswordEndpoint(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public override void Configure()
    {
        Post("/auth/reset-password");
        AllowAnonymous();
        Options(x => x
            .WithTags("Authentication")
            .WithSummary("Reset password with token")
            .WithDescription("Resets the user's password using a valid reset token"));
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        var command = new ResetPasswordCommand(req.Email, req.Token, req.NewPassword, req.ConfirmPassword);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            HttpContext.Response.StatusCode = 200;
            await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
            return;
        }

        if (result.Status == Ardalis.Result.ResultStatus.Invalid)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "Validation failed",
                    details = result.ValidationErrors.Select(e => new { field = e.Identifier, message = e.ErrorMessage })
                }
            }, ct);
            return;
        }

        if (result.Status == Ardalis.Result.ResultStatus.Error)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "INVALID_TOKEN",
                    message = result.Errors.FirstOrDefault() ?? "Invalid or expired token"
                }
            }, ct);
            return;
        }

        HttpContext.Response.StatusCode = 500;
        await HttpContext.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = "INTERNAL_ERROR",
                message = "An error occurred"
            }
        }, ct);
    }
}
