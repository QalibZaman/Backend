using System.Net;
using NettoRetail.API.Models.Error;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.Exceptions;

namespace NettoRetail.API.Infrastructure.Helpers.ErrorHandlers;

public static class ApiExceptionHandler
{
    public static ApiExceptionHandlerResponse Handle(Exception ex, HttpContext? httpContext = null)
    {
        return ex switch
        {
            NotFoundException e => HandleNotFoundException(e),
            BadRequestException e => HandleBadRequestException(e),
            ConflictException e => HandleConflictException(e),
            MyUnauthorizedException e => HandleMyUnauthorizedException(e),
            ForbiddenException e => HandleForbiddenException(e),
            PaymentExceptions e => HandlePaymentExceptions(e),
            InvalidTokenCredentialsException e => HandleTokenCredentialsException(e),
            FluentValidation.ValidationException e => HandleFluentValidationException(e),
            System.ComponentModel.DataAnnotations.ValidationException e => HandleDataAnnotationsValidationException(e),

            _ => HandleDefault(httpContext)
        };
    }

    private static ApiExceptionHandlerResponse HandleDefault(HttpContext? httpContext)
    {
        if (IsContentEditorInitializationRequest(httpContext))
        {
            return new ApiExceptionHandlerResponse(
                (int)HttpStatusCode.InternalServerError,
                GenerateResponse(
                    ContentEditorErrorCatalog.InitializationErrorMessage,
                    BuildTimestampMeta()));
        }

        if (IsContentEditorRecoveryRequest(httpContext))
        {
            var timestamp = DateTime.UtcNow;
            return new ApiExceptionHandlerResponse(
                (int)HttpStatusCode.InternalServerError,
                GenerateResponse(
                    ContentEditorErrorCatalog.BuildRecoveryErrorMessage(timestamp),
                    BuildTimestampMeta(timestamp)));
        }

        return new ApiExceptionHandlerResponse(
            (int)HttpStatusCode.InternalServerError,
            GenerateResponse("Unexpected error happened on the server while trying to process the request"));
    }

    private static ApiExceptionHandlerResponse HandleNotFoundException(NotFoundException ex)
    {
        return new ApiExceptionHandlerResponse((int)HttpStatusCode.NotFound, GenerateResponse(ex.Message));
    }

    private static ApiExceptionHandlerResponse HandleForbiddenException(ForbiddenException ex)
    {
        return new ApiExceptionHandlerResponse((int)HttpStatusCode.Forbidden, GenerateResponse(ex.Message));
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

    private static ApiExceptionHandlerResponse HandlePaymentExceptions(PaymentExceptions ex)
    {
        return new ApiExceptionHandlerResponse((int)HttpStatusCode.BadRequest, GenerateResponse(ex.Message));
    }

    private static ApiExceptionHandlerResponse HandleDataAnnotationsValidationException(System.ComponentModel.DataAnnotations.ValidationException ex)
    {
        var key = ex.ValidationResult?.MemberNames?.FirstOrDefault()?.ToLowerInvariant()
                  ?? "model";

        var errors = new Dictionary<string, string[]>
        {
            { key, new[] { ex.ValidationResult?.ErrorMessage ?? ex.Message } }
        };

        return BuildValidationResponse(errors);
    }

    private static ApiExceptionHandlerResponse HandleFluentValidationException(FluentValidation.ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName.ToLowerInvariant())
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return BuildValidationResponse(errors);
    }

    private static ApiExceptionHandlerResponse BuildValidationResponse(
    Dictionary<string, string[]> errors)
    {
        var response = new ApiResponse("Validation failed", errors);

        return new ApiExceptionHandlerResponse((int)HttpStatusCode.BadRequest, response);
    }

    private static ApiExceptionHandlerResponse HandleTokenCredentialsException(InvalidTokenCredentialsException ex)
    {
        return new ApiExceptionHandlerResponse((int)HttpStatusCode.BadRequest, GenerateResponse(ex.Message));
    }

    private static ApiResponse GenerateResponse(
        string message,
        Dictionary<string, string>? meta = null) => new(message, null, meta);

    private static bool IsContentEditorInitializationRequest(HttpContext? httpContext)
    {
        if (httpContext == null)
        {
            return false;
        }

        var path = httpContext.Request.Path.Value ?? string.Empty;
        var method = httpContext.Request.Method;

        return path.Contains("/content/block/", StringComparison.OrdinalIgnoreCase)
               && HttpMethods.IsGet(method);
    }

    private static bool IsContentEditorRecoveryRequest(HttpContext? httpContext)
    {
        if (httpContext == null)
        {
            return false;
        }

        var path = httpContext.Request.Path.Value ?? string.Empty;
        var method = httpContext.Request.Method;

        if (!path.Contains("/content/block/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return HttpMethods.IsPut(method)
               || (HttpMethods.IsPost(method) && path.EndsWith("/preview", StringComparison.OrdinalIgnoreCase));
    }

    private static Dictionary<string, string> BuildTimestampMeta(DateTime? timestampUtc = null)
    {
        var timestamp = timestampUtc ?? DateTime.UtcNow;

        return new Dictionary<string, string>
        {
            ["timestamp"] = timestamp.ToString("O"),
        };
    }
}
