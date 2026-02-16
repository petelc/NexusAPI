using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nexus.API.UseCases.Auth.DTOs;
using Shouldly;

namespace Nexus.API.FunctionalTests.Auth;

[Collection("Sequential")]
public class MeEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client;

  public MeEndpointTests(CustomWebApplicationFactory<Program> factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task Me_WithValidToken_ReturnsCurrentUser()
  {
    // Arrange - login to get a token
    var loginRequest = new LoginRequestDto(
      TestConstants.TestUserEmail,
      TestConstants.TestUserPassword);

    var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
    var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

    // Act
    _client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
    var response = await _client.GetAsync("/api/v1/auth/me");

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task Me_WithoutToken_Returns401()
  {
    // Arrange - no auth header
    var client = _client;
    client.DefaultRequestHeaders.Authorization = null;

    // Act
    var response = await client.GetAsync("/api/v1/auth/me");

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Me_WithInvalidToken_Returns401()
  {
    // Arrange
    _client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

    // Act
    var response = await _client.GetAsync("/api/v1/auth/me");

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }
}
