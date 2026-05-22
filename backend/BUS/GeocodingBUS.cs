using System.Text.Json;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.BUS;

public class GeocodingBUS
{
    private readonly HttpClient _httpClient;

    public GeocodingBUS(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GeocodingResultDTO?> ResolveAddressAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        var query = address;

        if (!query.Contains("Việt Nam", StringComparison.OrdinalIgnoreCase) &&
            !query.Contains("Vietnam", StringComparison.OrdinalIgnoreCase))
        {
            query += ", Hồ Chí Minh, Việt Nam";
        }

        var url =
            "https://nominatim.openstreetmap.org/search" +
            $"?format=json&limit=1&q={Uri.EscapeDataString(query)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.UserAgent.ParseAdd("VinhKhanhNarration/1.0");
        request.Headers.Referrer = new Uri("http://localhost:5151");

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
        {
            return null;
        }

        var first = root[0];

        var latText = first.GetProperty("lat").GetString();
        var lonText = first.GetProperty("lon").GetString();
        var displayName = first.TryGetProperty("display_name", out var displayNameProp)
            ? displayNameProp.GetString() ?? string.Empty
            : string.Empty;

        if (!decimal.TryParse(latText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lat))
        {
            return null;
        }

        if (!decimal.TryParse(lonText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lon))
        {
            return null;
        }

        return new GeocodingResultDTO
        {
            DisplayName = displayName,
            Latitude = lat,
            Longitude = lon
        };
    }
}