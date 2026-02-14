using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Entities;
using Nexus.API.Core.Interfaces;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Auth.DTOs;
using UserDto = Nexus.API.UseCases.Auth.DTOs.UserDto;

namespace Nexus.API.Web.Endpoints.Auth;

/// <summary>
/// Login user endpoint
/// POST /api/v1/auth/login
/// </summary>
public class LoginEndpoint : Endpoint<LoginRequestDto, AuthResponseDto>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly IJwtTokenService _jwtTokenService;
  private readonly IRefreshTokenRepository _refreshTokenRepository;

  public LoginEndpoint(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _jwtTokenService = jwtTokenService;
    _refreshTokenRepository = refreshTokenRepository;
  }

  public override void Configure()
  {
    Post("/auth/login");
    AllowAnonymous();

    Description(b => b
      .WithTags("Authentication")
      .WithSummary("Login user")
      .WithDescription("Authenticate user with email and password. Returns JWT tokens upon successful authentication."));
  }

  public override async Task HandleAsync(
    LoginRequestDto request,
    CancellationToken ct)
  {
    // Find user by email
    var user = await _userManager.FindByEmailAsync(request.Email);
    
    if (user == null)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Invalid email or password" }
      }, ct);
      return;
    }

    // Check if user is active
    if (!user.IsActive)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = "Account is deactivated. Please contact support." }
      }, ct);
      return;
    }

    // Verify password
    var result = await _signInManager.CheckPasswordSignInAsync(
      user,
      request.Password,
      lockoutOnFailure: true);

    if (!result.Succeeded)
    {
      string errorMessage = result.IsLockedOut 
        ? "Account locked due to multiple failed login attempts. Try again in 5 minutes."
        : result.IsNotAllowed
          ? "Login not allowed. Please confirm your email address."
          : "Invalid email or password";

      HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new { message = errorMessage }
      }, ct);
      return;
    }

    // Update last login
    user.LastLoginAt = DateTime.UtcNow;
    await _userManager.UpdateAsync(user);

    // Generate JWT tokens
    var roles = await _userManager.GetRolesAsync(user);
    var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
    var jwtId = _jwtTokenService.GetJwtIdFromToken(accessToken);
    var refreshToken = _jwtTokenService.GenerateRefreshToken();

    // Store refresh token in database
    var refreshTokenEntity = RefreshToken.Create(
      user.Id,
      refreshToken,
      jwtId!,
      daysValid: 7);

    await _refreshTokenRepository.AddAsync(refreshTokenEntity, ct);

    var response = new AuthResponseDto(
      accessToken,
      refreshToken,
      DateTime.UtcNow.AddMinutes(15),
      new UserDto(
        user.Id,
        user.Email!,
        user.UserName!,
        user.FirstName,
        user.LastName,
        user.AvatarUrl,
        user.EmailConfirmed,
        user.TwoFactorEnabled));

    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    await HttpContext.Response.WriteAsJsonAsync(response, ct);
  }
}
