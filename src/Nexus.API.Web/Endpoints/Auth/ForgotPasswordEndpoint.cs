using FastEndpoints;
using MediatR;
using Nexus.API.UseCases.Auth.Commands;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.Web.Endpoints.Auth;

/// <summary>
/// Initiates the forgot password flow.
/// POST /api/v1/auth/forgot-password
///
/// Always returns success to prevent email enumeration attacks.
/// If the email exists, a reset link is sent.
/// </summary>
public class ForgotPasswordEndpoint : Endpoint<ForgotPasswordRequest, ForgotPasswordResponse>
{
    private readonly IMediator _mediator;

    public ForgotPasswordEndpoint(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public override void Configure()
    {
        Post("/auth/forgot-password");
        AllowAnonymous();
        Options(x => x
            .WithTags("Authentication")
            .WithSummary("Request password reset")
            .WithDescription("Sends a password reset email if the account exists"));
    }

    public override async Task HandleAsync(ForgotPasswordRequest req, CancellationToken ct)
    {
        var command = new ForgotPasswordCommand(req.Email);
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

        HttpContext.Response.StatusCode = 500;
        await HttpContext.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = "INTERNAL_ERROR",
                message = result.Errors.FirstOrDefault() ?? "An error occurred"
            }
        }, ct);
    }
}
