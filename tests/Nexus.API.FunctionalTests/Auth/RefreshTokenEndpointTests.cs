using System.Net;
using System.Net.Http.Json;
using Nexus.API.UseCases.Auth.DTOs;
using Shouldly;

namespace Nexus.API.FunctionalTests.Auth;

[Collection("Sequential")]
public class RefreshTokenEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client;

  public RefreshTokenEndpointTests(CustomWebApplicationFactory<Program> factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task RefreshToken_WithValidTokens_ReturnsNewTokenPair()
  {
    // Arrange - login first to get tokens
    var loginRequest = new LoginRequestDto(
      TestConstants.TestUserEmail,
      TestConstants.TestUserPassword);

    var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
    var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

    var refreshRequest = new RefreshTokenRequestDto(
      auth!.AccessToken,
      auth.RefreshToken);

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var refreshed = await response.Content.ReadFromJsonAsync<RefreshTokenResponseDto>();
    refreshed.ShouldNotBeNull();
    refreshed.AccessToken.ShouldNotBeNullOrWhiteSpace();
    refreshed.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    // New tokens should be different from old ones (token rotation)
    refreshed.AccessToken.ShouldNotBe(auth.AccessToken);
    refreshed.RefreshToken.ShouldNotBe(auth.RefreshToken);
  }

  [Fact]
  public async Task RefreshToken_WithInvalidRefreshToken_Returns401()
  {
    // Arrange - login to get access token but use fake refresh token
    var loginRequest = new LoginRequestDto(
      TestConstants.TestUserEmail,
      TestConstants.TestUserPassword);

    var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
    var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

    var refreshRequest = new RefreshTokenRequestDto(
      auth!.AccessToken,
      "invalid-refresh-token");

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task RefreshToken_UsedTwice_SecondAttemptFails()
  {
    // Arrange - login and use refresh token once
    var loginRequest = new LoginRequestDto(
      TestConstants.TestUserEmail,
      TestConstants.TestUserPassword);

    var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
    var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

    var refreshRequest = new RefreshTokenRequestDto(
      auth!.AccessToken,
      auth.RefreshToken);

    // First refresh should succeed
    await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

    // Act - same refresh token used again
    var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

    // Assert - should fail because token was already used (rotation)
    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
