using NettoHealth.Application.Models.Response;

namespace NettoHealth.API.Models.Error;

public record ApiExceptionHandlerResponse(int Status, ServiceResponse Result);