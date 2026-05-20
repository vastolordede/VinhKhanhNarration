using System.Net;
using FluentAssertions;

namespace VinhKhanhNarration.Api.Tests.TestHelpers;

public static class HttpTestHelper
{
    public static async Task AssertSuccessWithBody(HttpResponseMessage response)
    {
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NoContent
        );

        if (response.StatusCode != HttpStatusCode.NoContent)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrWhiteSpace();
        }
    }

    public static async Task<string> ReadBody(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }
}
