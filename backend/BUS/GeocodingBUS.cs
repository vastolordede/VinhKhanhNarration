using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
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

        var query = NormalizeQuery(address);
        var houseNumber = ExtractHouseNumber(query);

        var url =
            "https://nominatim.openstreetmap.org/search" +
            "?format=json" +
            "&addressdetails=1" +
            "&limit=5" +
            "&countrycodes=vn" +
            "&accept-language=vi" +
            $"&q={Uri.EscapeDataString(query)}";

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

        var selected = SelectBestResult(root, query, houseNumber);

        if (selected == null)
        {
            return null;
        }

        if (!decimal.TryParse(
                selected.LatText,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var lat))
        {
            return null;
        }

        if (!decimal.TryParse(
                selected.LonText,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var lon))
        {
            return null;
        }

        var isExactHouseNumber =
            string.IsNullOrWhiteSpace(houseNumber) ||
            ContainsHouseNumber(selected.DisplayName, houseNumber);

        return new GeocodingResultDTO
        {
            DisplayName = selected.DisplayName,
            Latitude = lat,
            Longitude = lon,
            IsExactHouseNumber = isExactHouseNumber,
            MatchQuality = isExactHouseNumber ? "exact_house_number" : "approximate",
            Warning = isExactHouseNumber
                ? null
                : "Nominatim không tìm thấy đúng số nhà. Tọa độ này có thể chỉ là vị trí gần đúng của đường hoặc khu vực."
        };
    }

    private static string NormalizeQuery(string address)
    {
        var query = address.Trim();

        if (!query.Contains("Việt Nam", StringComparison.OrdinalIgnoreCase) &&
            !query.Contains("Vietnam", StringComparison.OrdinalIgnoreCase))
        {
            query += ", Hồ Chí Minh, Việt Nam";
        }

        return query;
    }

    private static GeocodingCandidate? SelectBestResult(
        JsonElement root,
        string query,
        string? houseNumber)
    {
        GeocodingCandidate? best = null;

        for (int i = 0; i < root.GetArrayLength(); i++)
        {
            var item = root[i];

            var latText = item.TryGetProperty("lat", out var latProp)
                ? latProp.GetString()
                : null;

            var lonText = item.TryGetProperty("lon", out var lonProp)
                ? lonProp.GetString()
                : null;

            var displayName = item.TryGetProperty("display_name", out var displayNameProp)
                ? displayNameProp.GetString() ?? string.Empty
                : string.Empty;

            if (string.IsNullOrWhiteSpace(latText) || string.IsNullOrWhiteSpace(lonText))
            {
                continue;
            }

            var score = ScoreResult(displayName, query, houseNumber);

            var candidate = new GeocodingCandidate
            {
                LatText = latText,
                LonText = lonText,
                DisplayName = displayName,
                Score = score
            };

            if (best == null || candidate.Score > best.Score)
            {
                best = candidate;
            }
        }

        return best;
    }

    private static int ScoreResult(string displayName, string query, string? houseNumber)
    {
        var score = 0;

        if (displayName.Contains("Vĩnh Khánh", StringComparison.OrdinalIgnoreCase))
        {
            score += 50;
        }

        if (displayName.Contains("Khánh Hội", StringComparison.OrdinalIgnoreCase))
        {
            score += 20;
        }

        if (displayName.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) ||
            displayName.Contains("Ho Chi Minh", StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(houseNumber) &&
            ContainsHouseNumber(displayName, houseNumber))
        {
            score += 100;
        }

        if (!string.IsNullOrWhiteSpace(houseNumber) &&
            !ContainsHouseNumber(displayName, houseNumber))
        {
            score -= 30;
        }

        return score;
    }

    private static string? ExtractHouseNumber(string text)
    {
        var match = Regex.Match(text, @"^\s*(\d+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static bool ContainsHouseNumber(string text, string houseNumber)
    {
        return Regex.IsMatch(
            text,
            $@"(^|[^\d]){Regex.Escape(houseNumber)}([^\d]|$)",
            RegexOptions.IgnoreCase);
    }

    private sealed class GeocodingCandidate
    {
        public string LatText { get; set; } = string.Empty;
        public string LonText { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int Score { get; set; }
    }
}