using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using VinhKhanhNarration.Api.Tests.TestHelpers;

namespace VinhKhanhNarration.Api.Tests.Integration;

public class PublicFlowSmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PublicFlowSmokeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateGuestSession_ShouldReturnGuestSessionId()
    {
        var request = new
        {
            deviceInfo = "integration-test-browser",
            ipAddress = "127.0.0.1"
        };

        var response = await _client.PostAsJsonAsync("/api/public/guest-sessions", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("GUEST");
    }

    [Fact]
    public async Task PublicMainFlow_ShouldCreateGuestSession_LoadData_ResolveQr_AndSubmitFeedback()
    {
        var guestSessionId = await CreateGuestSession();
        var languageId = await GetVietnameseLanguageId();
        var placeId = await GetFirstPlaceId();

        guestSessionId.Should().NotBeNullOrWhiteSpace();
        languageId.Should().BeGreaterThan(0);
        placeId.Should().BeGreaterThan(0);

        await ChangeGuestLanguage(guestSessionId, languageId);
        await ResolveQr(guestSessionId, languageId);
        await CheckGeofence(guestSessionId, languageId);
        await SubmitFeedback(guestSessionId, placeId);
    }

    private async Task<string> CreateGuestSession()
    {
        var response = await _client.PostAsJsonAsync("/api/public/guest-sessions", new
        {
            deviceInfo = "integration-test-browser",
            ipAddress = "127.0.0.1"
        });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);

        var guestSessionId = JsonTestHelper.FindFirstString(
            document.RootElement,
            "guestSessionId",
            "GuestSessionId",
            "id"
        );

        guestSessionId.Should().NotBeNullOrWhiteSpace();
        return guestSessionId!;
    }

    private async Task<long> GetVietnameseLanguageId()
    {
        var response = await _client.GetAsync("/api/languages/active");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);

        var vi = JsonTestHelper.FindFirstObjectWhereStringEquals(document.RootElement, "languageCode", "vi")
                 ?? JsonTestHelper.FindFirstObjectWhereStringEquals(document.RootElement, "LanguageCode", "vi");

        var source = vi ?? document.RootElement;
        var languageId = JsonTestHelper.FindFirstLong(source, "languageId", "LanguageId", "id");

        languageId.Should().NotBeNull();
        return languageId!.Value;
    }

    private async Task<long> GetFirstPlaceId()
    {
        var response = await _client.GetAsync("/api/places/active");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);

        var placeId = JsonTestHelper.FindFirstLong(document.RootElement, "placeId", "PlaceId", "id");

        placeId.Should().NotBeNull();
        return placeId!.Value;
    }

    private async Task ChangeGuestLanguage(string guestSessionId, long languageId)
    {
        var response = await _client.PatchAsJsonAsync($"/api/public/guest-sessions/{guestSessionId}/language", new
        {
            languageId
        });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.NotFound
        );
    }

    private async Task ResolveQr(string guestSessionId, long languageId)
    {
        var response = await _client.PostAsJsonAsync("/api/public/qr/resolve", new
        {
            qrCodeValue = "QR_PLACE_OC_VINH_KHANH",
            languageId,
            guestSessionId
        });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrWhiteSpace();
        }
    }

    private async Task CheckGeofence(string guestSessionId, long languageId)
    {
        var response = await _client.PostAsJsonAsync("/api/public/geofence/check", new
        {
            guestSessionId,
            latitude = 10.7569,
            longitude = 106.7057,
            languageId
        });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest
        );

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrWhiteSpace();
        }
    }

    private async Task SubmitFeedback(string guestSessionId, long placeId)
    {
        var response = await _client.PostAsJsonAsync("/api/public/feedbacks", new
        {
            guestSessionId,
            placeId,
            dishId = (long?)null,
            narrationId = (long?)null,
            rating = 5,
            comment = "Integration test feedback"
        });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest
        );
    }
}
