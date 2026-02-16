using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Nexus.API.Core.Aggregates.UserAggregate;
using Nexus.API.Core.Entities;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.ValueObjects;
using Nexus.API.Infrastructure.Identity;
using Nexus.API.UseCases.Auth.DTOs;
using UserDto = Nexus.API.UseCases.Auth.DTOs.UserDto;

namespace Nexus.API.Web.Endpoints.Auth;

/// <summary>
/// Register a new user endpoint
/// POST /api/v1/auth/register
/// </summary>
public class RegisterEndpoint : Endpoint<RegisterRequestDto, AuthResponseDto>
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IJwtTokenService _jwtTokenService;
  private readonly IRefreshTokenRepository _refreshTokenRepository;
  private readonly IUserRepository _userRepository;

  public RegisterEndpoint(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository)
  {
    _userManager = userManager;
    _jwtTokenService = jwtTokenService;
    _refreshTokenRepository = refreshTokenRepository;
    _userRepository = userRepository;
  }

  public override void Configure()
  {
    Post("/auth/register");
    AllowAnonymous();

    Description(b => b
      .WithTags("Authentication")
      .WithSummary("Register a new user")
      .WithDescription("Creates a new user account with email, username, and password. Returns JWT tokens upon successful registration."));
  }

  public override async Task HandleAsync(
    RegisterRequestDto request,
    CancellationToken ct)
  {
    // Validate passwords match
    if (request.Password != request.ConfirmPassword)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
      await HttpContext.Response.WriteAsJsonAsync(new 
      { 
        error = new 
        { 
          message = "Passwords do not match",
          field = "confirmPassword"
        }
      }, ct);
      return;
    }

    // Create user
    var user = new ApplicationUser
    {
      Id = Guid.NewGuid(),
      Email = request.Email,
      UserName = request.Username,
      FirstName = request.FirstName,
      LastName = request.LastName,
      EmailConfirmed = false,
      CreatedAt = DateTime.UtcNow,
      IsActive = true
    };

    var result = await _userManager.CreateAsync(user, request.Password);

    if (!result.Succeeded)
    {
      HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
      await HttpContext.Response.WriteAsJsonAsync(new
      {
        error = new
        {
          message = "Registration failed",
          errors = result.Errors.Select(e => new { code = e.Code, description = e.Description })
        }
      }, ct);
      return;
    }

    // Assign default role
    await _userManager.AddToRoleAsync(user, "Viewer");

    // Create corresponding domain user in dbo.Users (same ID for FK relationships)
    var domainUser = Nexus.API.Core.Aggregates.UserAggregate.User.CreateFromIdentity(
      user.Id,
      new Email(user.Email!),
      user.UserName!,
      new PersonName(user.FirstName, user.LastName),
      user.PasswordHash ?? string.Empty);

    await _userRepository.AddAsync(domainUser, ct);

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

    HttpContext.Response.StatusCode = StatusCodes.Status201Created;
    await HttpContext.Response.WriteAsJsonAsync(response, ct);
  }
}
