using System.Net;
using System.Net.Http.Json;
using Nexus.API.UseCases.Auth.DTOs;
using Shouldly;

namespace Nexus.API.FunctionalTests.Auth;

[Collection("Sequential")]
public class RegisterEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client;

  public RegisterEndpointTests(CustomWebApplicationFactory<Program> factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task Register_WithValidData_Returns201WithTokens()
  {
    // Arrange
    var uniqueId = Guid.NewGuid().ToString("N")[..8];
    var request = new RegisterRequestDto(
      $"newuser_{uniqueId}@nexus.dev",
      $"newuser_{uniqueId}",
      "New",
      "User",
      "ValidPass123!",
      "ValidPass123!");

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Created);

    var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    auth.ShouldNotBeNull();
    auth.AccessToken.ShouldNotBeNullOrWhiteSpace();
    auth.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    auth.User.ShouldNotBeNull();
    auth.User.Email.ShouldBe($"newuser_{uniqueId}@nexus.dev");
    auth.User.Username.ShouldBe($"newuser_{uniqueId}");
    auth.User.FirstName.ShouldBe("New");
    auth.User.LastName.ShouldBe("User");
  }

  [Fact]
  public async Task Register_WithDuplicateEmail_Returns400()
  {
    // Arrange - use the seeded test user's email
    var request = new RegisterRequestDto(
      TestConstants.TestUserEmail,
      "anotheruser",
      "Another",
      "User",
      "ValidPass123!",
      "ValidPass123!");

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithMismatchedPasswords_Returns400()
  {
    // Arrange
    var request = new RegisterRequestDto(
      "mismatch@nexus.dev",
      "mismatchuser",
      "Mis",
      "Match",
      "Password123!",
      "DifferentPass123!");

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_WithWeakPassword_Returns400()
  {
    // Arrange - password too short and missing requirements
    var request = new RegisterRequestDto(
      "weakpass@nexus.dev",
      "weakpassuser",
      "Weak",
      "Pass",
      "short",
      "short");

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Register_NewUser_CanLoginImmediately()
  {
    // Arrange - register a new user
    var uniqueId = Guid.NewGuid().ToString("N")[..8];
    var registerRequest = new RegisterRequestDto(
      $"logintest_{uniqueId}@nexus.dev",
      $"logintest_{uniqueId}",
      "Login",
      "Test",
      "ValidPass123!",
      "ValidPass123!");

    await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

    // Act - login with the new credentials
    var loginRequest = new LoginRequestDto(
      $"logintest_{uniqueId}@nexus.dev",
      "ValidPass123!");

    var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

    // Assert
    loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
    auth.ShouldNotBeNull();
    auth.AccessToken.ShouldNotBeNullOrWhiteSpace();
  }
}
