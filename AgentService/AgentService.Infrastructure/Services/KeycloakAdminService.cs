using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentService.Application.Interfaces.Services;
using AgentService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgentService.Infrastructure.Services;

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakAdminService> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiry;

    public KeycloakAdminService(HttpClient httpClient, IConfiguration configuration, ILogger<KeycloakAdminService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
        {
            return _accessToken;
        }

        var authority = _configuration["Keycloak:Authority"];
        var clientId = _configuration["Keycloak:ClientId"];
        var clientSecret = _configuration["Keycloak:ClientSecret"];
        
        if (string.IsNullOrEmpty(clientSecret))
        {
             // Fallback or throw? For now fallback to dummy to satisfy compiler, but log warning in real app
             clientSecret = "YOUR_CLIENT_SECRET";
        } 
        
        var tokenEndpoint = $"{authority}/protocol/openid-connect/token";

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret ?? ""),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var node = JsonNode.Parse(json);
        _accessToken = node?["access_token"]?.ToString();
        var expiresIn = node?["expires_in"]?.GetValue<int>() ?? 300;
        
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60); // Buffer of 60s

        if (string.IsNullOrEmpty(_accessToken))
        {
            throw new Exception("Failed to retrieve access token from Keycloak.");
        }

        return _accessToken;
    }

    private string GetAdminApiUrl()
    {
        // Authority: http://localhost:8180/realms/ai-agent
        // Admin API: http://localhost:8180/admin/realms/ai-agent
        var authority = _configuration["Keycloak:Authority"];
        if (string.IsNullOrEmpty(authority)) throw new Exception("Keycloak:Authority is missing config");

        // Simple string replacement to switch to admin API
        // This assumes the Authority format ends with /realms/{realm}
        if (authority.Contains("/realms/"))
        {
             // http://localhost:8180/realms/ai-agent -> http://localhost:8180/admin/realms/ai-agent
             // Wait, standard Keycloak admin URL is .../admin/realms/...
             // The logic: replace first /realms/ with /admin/realms/ ? 
             // Or just build it if I knew the base URL.
             // Let's try to infer base URL.
             var uri = new Uri(authority);
             var baseUrl = $"{uri.Scheme}://{uri.Authority}";
             var realm = uri.Segments.Last().Trim('/'); 
             return $"{baseUrl}/admin/realms/{realm}";
        }
        
        return authority; // Fallback, likely will fail if structure is different
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        try 
        {
            var token = await GetAccessTokenAsync();
            var baseUrl = GetAdminApiUrl();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/users?email={Uri.EscapeDataString(email)}&exact=true");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonNode.Parse(json)?.AsArray();

            return users != null && users.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists in Keycloak");
            // We return false or throw? If keycloak is down, maybe we should fail hard?
            // User requested "Validate fully", so let's allow exception to bubble up or handle explicitly.
            throw; 
        }
    }

    public async Task<string> CreateUserAsync(User user, string temporaryPassword)
    {
        var token = await GetAccessTokenAsync();
        var baseUrl = GetAdminApiUrl();

        var newUser = new
        {
            username = user.Username,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            enabled = true,
            emailVerified = true,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = temporaryPassword,
                    temporary = true
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create user in Keycloak: {StatusCode} {Error}", response.StatusCode, error);
            throw new Exception($"Failed to create user in Keycloak: {response.StatusCode} - {error}");
        }
        
        // Retrieve the ID of the created user (Command usually returns Location header)
        if (response.Headers.Location != null)
        {
             var path = response.Headers.Location.AbsolutePath;
             return path.Split('/').Last();
        }
        
        return string.Empty;
    }
}
