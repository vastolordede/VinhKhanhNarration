using VinhKhanhNarration.Api.Tests.TestHelpers;
using FluentAssertions;
namespace VinhKhanhNarration.Api.Tests.Integration;

public class AdminAndLogReadSmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminAndLogReadSmokeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    public static IEnumerable<object[]> ReadEndpoints()
    {
        yield return new object[] { "/api/admin-users" };
        yield return new object[] { "/api/languages" };
        yield return new object[] { "/api/place-types" };
        yield return new object[] { "/api/content-types" };
        yield return new object[] { "/api/target-types" };
        yield return new object[] { "/api/translation-sources" };
        yield return new object[] { "/api/trigger-modes" };
        yield return new object[] { "/api/geofence-event-types" };
        yield return new object[] { "/api/geofence-event-statuses" };
        yield return new object[] { "/api/places" };
        yield return new object[] { "/api/dish-categories" };
        yield return new object[] { "/api/dishes" };
        yield return new object[] { "/api/place-dishes" };
        yield return new object[] { "/api/narration-contents" };
        yield return new object[] { "/api/narration-translations" };
        yield return new object[] { "/api/audio-files" };
        yield return new object[] { "/api/qr-codes" };
        yield return new object[] { "/api/admin/feedbacks" };
        yield return new object[] { "/api/admin/feedbacks/pending" };
        yield return new object[] { "/api/admin/listening-histories" };
        yield return new object[] { "/api/admin/geofence-events" };
    }

    [Theory]
    [MemberData(nameof(ReadEndpoints))]
    public async Task AdminOrLogReadEndpoint_ShouldNotCrash(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);

        // These generated controllers are currently not protected by JWT in the PoC.
        // If you add authorization later, change OK to Unauthorized/Forbidden as expected.
        response.IsSuccessStatusCode.Should().BeTrue($"Endpoint {endpoint} should return 2xx in current PoC.");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNull();
    }
}
