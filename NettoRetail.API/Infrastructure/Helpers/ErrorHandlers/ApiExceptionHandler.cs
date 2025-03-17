using NettoRetail.API.Models.Error;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.Exceptions;
using NettoRetail.Shared.Exceptions;
using System.Net;

namespace NettoRetail.API.Infrastructure.Helpers.ErrorHandlers;

public static class ApiExceptionHandler
{
    private static readonly Dictionary<Type, Func<Exception, ApiExceptionHandlerResponse>> Handlers =
        new()
        {
            { typeof(NotFoundException), ex => HandleNotFoundException((NotFoundException)ex) },
            { typeof(BadRequestException), ex => HandleBadRequestException((BadRequestException)ex) },
            { typeof(ConflictException), ex => HandleConflictException((ConflictException)ex) },
            { typeof(MyUnauthorizedException), ex => HandleMyUnauthorizedException((MyUnauthorizedException)ex) },
            { typeof(EncryptionException), ex => HandleEncryptionException((EncryptionException)ex) }
        };

    public static ApiExceptionHandlerResponse Handle(Exception ex)
    {
        return Handlers.TryGetValue(ex.GetType(), out var handler)
            ? handler(ex)
            : HandleDefault(ex);
    }

    private static ApiExceptionHandlerResponse HandleDefault(Exception ex)
    {
        return new ApiExceptionHandlerResponse(
            (int)HttpStatusCode.InternalServerError,
            GenerateResponse("Unexpected error happened on the server while trying to process the request"));
    }

    private static ApiExceptionHandlerResponse HandleNotFoundException(NotFoundException ex)
    {
        return new ApiExceptionHandlerResponse((int)HttpStatusCode.NotFound, GenerateResponse(ex.Message));
    }

    private static ApiExceptionHandlerResponse HandleBadRequestException(BadRequestException ex)
    {
        return new ApiExceptionHandlerResponse((int)HttpStatusCode.BadRequest, GenerateResponse(ex.Message));
    }

    private static ApiExceptionHandlerResponse HandleConflictException(ConflictException ex)
    {
        return new ApiExceptionHandlerResponse((int)HttpStatusCode.Conflict, GenerateResponse(ex.Message));
    }

    private static ApiExceptionHandlerResponse HandleMyUnauthorizedException(MyUnauthorizedException ex)
    {
        return new ApiExceptionHandlerResponse((int)HttpStatusCode.Unauthorized, GenerateResponse(ex.Message));
    }

    private static ApiExceptionHandlerResponse HandleEncryptionException(EncryptionException ex)
    {
        return new ApiExceptionHandlerResponse((int)HttpStatusCode.BadRequest,
            GenerateResponse(ex.Message));
    }

    private static ApiResponse GenerateResponse(string message) => new(message);
}