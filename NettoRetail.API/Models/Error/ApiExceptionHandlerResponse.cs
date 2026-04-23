using NettoRetail.API.Models.Response;

namespace NettoRetail.API.Models.Error;

public record ApiExceptionHandlerResponse(int Status, ApiResponse Result);
