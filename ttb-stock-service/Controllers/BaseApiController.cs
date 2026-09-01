using Microsoft.AspNetCore.Mvc;
using ttb_stock_service.Models.Common;

namespace ttb_stock_service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "Success")
    {
        return Ok(ApiResponse<T>.SuccessResult(data, message));
    }

    protected ActionResult<ApiResponse> Success(string message = "Success")
    {
        return Ok(ApiResponse.SuccessResult(message));
    }

    protected ActionResult<ApiResponse<T>> CreatedSuccess<T>(string uri, T data, string message = "Resource created successfully")
    {
        return Created(uri, ApiResponse<T>.SuccessResult(data, message));
    }

    protected ActionResult<ApiResponse<T>> NotFoundError<T>(string message = "Resource not found")
    {
        return NotFound(ApiResponse<T>.FailureResult(message));
    }

    protected ActionResult<ApiResponse> NotFoundError(string message = "Resource not found")
    {
        return NotFound(ApiResponse.FailureResult(message));
    }

    protected ActionResult<ApiResponse<T>> BadRequestError<T>(string message, List<string>? errors = null)
    {
        return BadRequest(ApiResponse<T>.FailureResult(message, errors));
    }

    protected ActionResult<ApiResponse> BadRequestError(string message, List<string>? errors = null)
    {
        return BadRequest(ApiResponse.FailureResult(message, errors));
    }
}
