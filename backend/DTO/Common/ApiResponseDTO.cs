namespace VinhKhanhNarration.Api.DTO.Common;

public class ApiResponseDTO<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponseDTO<T> Ok(T? data, string message = "Success") => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponseDTO<T> Fail(string message) => new()
    {
        Success = false,
        Message = message,
        Data = default
    };
}
