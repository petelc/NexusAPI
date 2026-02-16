using System.Net.Http.Headers;
using System.Net.Http.Json;
using Nexus.API.UseCases.Auth.DTOs;

namespace Nexus.API.FunctionalTests;

/// <summary>
/// Helper methods for authenticated API requests in functional tests.
/// </summary>
public static class AuthHelper
{
  /// <summary>
  /// Login the test user and return the access token.
  /// </summary>
  public static async Task<string> GetAccessTokenAsync(HttpClient client)
  {
    var loginRequest = new LoginRequestDto(
      TestConstants.TestUserEmail,
      TestConstants.TestUserPassword);

    var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
    response.EnsureSuccessStatusCode();

    var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    return auth!.AccessToken;
  }

  /// <summary>
  /// Create an HttpClient with authentication headers set.
  /// </summary>
  public static async Task<HttpClient> CreateAuthenticatedClientAsync(
    CustomWebApplicationFactory<Program> factory)
  {
    var client = factory.CreateClient();
    var token = await GetAccessTokenAsync(client);
    client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", token);
    return client;
  }
}
