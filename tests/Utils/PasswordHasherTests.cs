using FluentAssertions;
using VinhKhanhNarration.Api.Utils;

namespace VinhKhanhNarration.Api.Tests.Utils;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ShouldReturnDifferentValue_FromPlainPassword()
    {
        var hasher = new PasswordHasher();
        const string plainPassword = "Admin@123";

        var hash = hasher.HashPassword(plainPassword);

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(plainPassword);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordIsCorrect()
    {
        var hasher = new PasswordHasher();
        const string plainPassword = "Admin@123";
        var hash = hasher.HashPassword(plainPassword);

        var result = hasher.VerifyPassword(plainPassword, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsWrong()
    {
        var hasher = new PasswordHasher();
        const string plainPassword = "Admin@123";
        const string wrongPassword = "WrongPassword@123";
        var hash = hasher.HashPassword(plainPassword);

        var result = hasher.VerifyPassword(wrongPassword, hash);

        result.Should().BeFalse();
    }
}
