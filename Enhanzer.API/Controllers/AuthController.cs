using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Enhanzer.API.Data;
using Enhanzer.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Enhanzer.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IHttpClientFactory httpClientFactory,
    AppDbContext dbContext,
    IConfiguration configuration,
    ILogger<AuthController> logger) : ControllerBase
{
    private const string ExternalApiUrl = "https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke";

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var payload = new ExternalApiPayload
        {
            CompanyCode = request.Email,
            ApiBody = new ExternalApiBody { Username = request.Email, Password = request.Password }
        };

        try
        {
            using var response = await httpClientFactory.CreateClient().PostAsJsonAsync(
                ExternalApiUrl, payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized(new { message = "The external service rejected the login." });
            }

            using var responseDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            await SaveLocationsAsync(responseDocument.RootElement, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "External login service could not be reached.");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Login service unavailable." });
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "External login service returned invalid JSON.");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Invalid login service response." });
        }

        var token = CreateToken(request.Email);
        return Ok(new { token });
    }

    private async Task SaveLocationsAsync(JsonElement root, CancellationToken cancellationToken)
    {
        if (!TryFindProperty(root, "User_Locations", out var locations) ||
            locations.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var existingCodes = (await dbContext.Locations
            .Select(location => location.LocationCode)
            .ToListAsync(cancellationToken)).ToHashSet();

        foreach (var location in locations.EnumerateArray())
        {
            var code = GetString(location, "Location_Code", "LocationCode", "Code");
            var name = GetString(location, "Location_Name", "LocationName", "Name");
            if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name) &&
                existingCodes.Add(code))
            {
                dbContext.Locations.Add(new LocationDetail { LocationCode = code, LocationName = name });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private string CreateToken(string email)
    {
        var secret = configuration["Jwt:Secret"] ??
                     throw new InvalidOperationException("Jwt:Secret is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.Name, email), new Claim(ClaimTypes.Email, email) };
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool TryFindProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty("data", out var data) &&
            TryFindProperty(data, propertyName, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static string GetString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }
}
