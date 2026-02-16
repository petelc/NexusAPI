using System.Net;
using System.Net.Http.Json;
using Nexus.API.UseCases.Auth.DTOs;
using Shouldly;

namespace Nexus.API.FunctionalTests.Auth;

[Collection("Sequential")]
public class ForgotPasswordEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client;

  public ForgotPasswordEndpointTests(CustomWebApplicationFactory<Program> factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task ForgotPassword_WithExistingEmail_Returns200()
  {
    // Arrange
    var request = new ForgotPasswordRequest
    {
      Email = TestConstants.TestUserEmail
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
    result.ShouldNotBeNull();
    result.Message.ShouldNotBeNullOrWhiteSpace();
  }

  [Fact]
  public async Task ForgotPassword_WithNonExistentEmail_StillReturns200()
  {
    // Arrange - should not reveal whether email exists
    var request = new ForgotPasswordRequest
    {
      Email = "nonexistent@nexus.dev"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

    // Assert - always returns 200 to prevent email enumeration
    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

  [Fact]
  public async Task ForgotPassword_WithEmptyEmail_Returns400()
  {
    // Arrange
    var request = new ForgotPasswordRequest
    {
      Email = ""
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
