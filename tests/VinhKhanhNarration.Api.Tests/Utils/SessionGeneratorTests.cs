using FluentAssertions;
using VinhKhanhNarration.Api.Utils;

namespace VinhKhanhNarration.Api.Tests.Utils;

public class SessionGeneratorTests
{
    [Fact]
    public void GenerateGuestSessionId_ShouldReturnNonEmptyValue()
    {
        var generator = new SessionGenerator();

        var sessionId = generator.GenerateGuestSessionId();

        sessionId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateGuestSessionId_ShouldGenerateUniqueValues()
    {
        var generator = new SessionGenerator();

        var first = generator.GenerateGuestSessionId();
        var second = generator.GenerateGuestSessionId();

        first.Should().NotBe(second);
    }

    [Fact]
    public void GenerateGuestSessionId_ShouldContainGuestPrefix()
    {
        var generator = new SessionGenerator();

        var sessionId = generator.GenerateGuestSessionId();

        sessionId.Should().StartWith("GUEST");
    }
}
