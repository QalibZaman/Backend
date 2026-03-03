using System.Net;
using NettoRetail.API.Models.Error;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.Exceptions;

namespace NettoRetail.API.Infrastructure.Helpers.ErrorHandlers;

public static class ApiExceptionHandler
{
    public static ApiExceptionHandlerResponse Handle(Exception ex)
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

            _ => HandleDefault(ex)
        };
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

    private static ApiResponse GenerateResponse(string message) => new(message);
}