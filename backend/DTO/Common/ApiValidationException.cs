namespace VinhKhanhNarration.Api.DTO.Common;

public class ApiValidationException : Exception
{
    public Dictionary<string, string> FieldErrors { get; }

    public ApiValidationException(
        Dictionary<string, string> fieldErrors,
        string message = "Validation failed.")
        : base(message)
    {
        FieldErrors = fieldErrors;
    }
}