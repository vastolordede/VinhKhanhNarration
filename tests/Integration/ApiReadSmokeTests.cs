using FluentAssertions;
using VinhKhanhNarration.Api.Tests.TestHelpers;

namespace VinhKhanhNarration.Api.Tests.Integration;

public class ApiReadSmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiReadSmokeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    public static IEnumerable<object[]> PublicReadEndpoints()
    {
        yield return new object[] { "/api/languages/active" };
        yield return new object[] { "/api/place-types/active" };
        yield return new object[] { "/api/content-types/active" };
        yield return new object[] { "/api/target-types/active" };
        yield return new object[] { "/api/translation-sources/active" };
        yield return new object[] { "/api/trigger-modes/active" };
        yield return new object[] { "/api/geofence-event-types/active" };
        yield return new object[] { "/api/geofence-event-statuses/active" };
        yield return new object[] { "/api/places/active" };
        yield return new object[] { "/api/dish-categories/active" };
        yield return new object[] { "/api/dishes/active" };
        yield return new object[] { "/api/dishes/signature" };
        yield return new object[] { "/api/narration-contents/active" };
        yield return new object[] { "/api/narration-translations/active" };
        yield return new object[] { "/api/audio-files/active" };
        yield return new object[] { "/api/qr-codes/active" };
    }

    [Theory]
    [MemberData(nameof(PublicReadEndpoints))]
    public async Task PublicReadEndpoint_ShouldReturnSuccess(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);

        await HttpTestHelper.AssertSuccessWithBody(response);
    }

    [Fact]
    public async Task SwaggerJson_ShouldReturnSuccess()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Vinh");
    }
}
