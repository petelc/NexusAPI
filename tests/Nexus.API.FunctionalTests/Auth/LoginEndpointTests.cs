using System.Net;
using System.Net.Http.Json;
using Nexus.API.UseCases.Auth.DTOs;
using Shouldly;

namespace Nexus.API.FunctionalTests.Auth;

[Collection("Sequential")]
public class LoginEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client;

  public LoginEndpointTests(CustomWebApplicationFactory<Program> factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task Login_WithValidCredentials_ReturnsTokensAndUser()
  {
    // Arrange
    var request = new LoginRequestDto(
      TestConstants.TestUserEmail,
      TestConstants.TestUserPassword);

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);

    var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    auth.ShouldNotBeNull();
    auth.AccessToken.ShouldNotBeNullOrWhiteSpace();
    auth.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    auth.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);
    auth.User.ShouldNotBeNull();
    auth.User.Email.ShouldBe(TestConstants.TestUserEmail);
    auth.User.Username.ShouldBe(TestConstants.TestUserUsername);
    auth.User.FirstName.ShouldBe(TestConstants.TestUserFirstName);
    auth.User.LastName.ShouldBe(TestConstants.TestUserLastName);
  }

  [Fact]
  public async Task Login_WithWrongPassword_Returns401()
  {
    // Arrange
    var request = new LoginRequestDto(
      TestConstants.TestUserEmail,
      "WrongPassword123!");

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Login_WithNonExistentEmail_Returns401()
  {
    // Arrange
    var request = new LoginRequestDto(
      "nonexistent@nexus.dev",
      "AnyPassword123!");

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Login_WithEmptyBody_ReturnsBadRequest()
  {
    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { });

    // Assert
    // FastEndpoints will return 400 for missing required fields
    response.StatusCode.ShouldBeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
  }
}
