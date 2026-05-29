namespace VinhKhanhNarration.Api.DTO;

public class GeocodingRequestDTO
{
    public string Address { get; set; } = string.Empty;
}

public class GeocodingResultDTO
{
    public string DisplayName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public bool IsExactHouseNumber { get; set; }
    public string MatchQuality { get; set; } = "approximate";
    public string? Warning { get; set; }
}