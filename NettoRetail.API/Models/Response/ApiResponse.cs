namespace NettoRetail.API.Models.Response;

public record ApiResponse(
    string Message,
    Dictionary<string, string[]>? Errors = null,
    Dictionary<string, string>? Meta = null);

public record ApiResponse<TModel>(TModel Data);
