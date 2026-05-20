using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.DTO.Common;

namespace VinhKhanhNarration.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult OkData<T>(T data, string message = "Success")
        => Ok(ApiResponseDTO<T>.Ok(data, message));

    protected IActionResult CreatedData<T>(T data, string message = "Created")
        => StatusCode(StatusCodes.Status201Created, ApiResponseDTO<T>.Ok(data, message));

    protected IActionResult BadRequestMessage(string message)
        => BadRequest(ApiResponseDTO<object>.Fail(message));

    protected IActionResult NotFoundMessage(string message = "Not found")
        => NotFound(ApiResponseDTO<object>.Fail(message));
}
