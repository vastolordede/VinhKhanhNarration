using System.Text.RegularExpressions;

namespace VinhKhanhNarration.Api.Utils;

public static class ValidationHelper
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    public static bool IsValidRating(int rating) => rating is >= 1 and <= 5;
}
