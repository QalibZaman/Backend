using NettoRetail.API.Models.Response;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.API.Models.Error;

public record ApiExceptionHandlerResponse(int Status, ApiResponse Result);