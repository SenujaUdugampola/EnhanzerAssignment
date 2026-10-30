using System.Text.Json.Serialization;

namespace Enhanzer.API.Models;

public class ExternalApiPayload
{
    [JsonPropertyName("API_Action")]
    public string ApiAction { get; set; } = "GetLoginData";

    [JsonPropertyName("Device_Id")]
    public string DeviceId { get; set; } = "D001";

    [JsonPropertyName("Sync_Time")]
    public string SyncTime { get; set; } = string.Empty;

    [JsonPropertyName("Company_Code")]
    public string CompanyCode { get; set; } = string.Empty;

    [JsonPropertyName("API_Body")]
    public ExternalApiBody ApiBody { get; set; } = new();
}

public class ExternalApiBody
{
    [JsonPropertyName("Username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("Pw")]
    public string Password { get; set; } = string.Empty;
}
