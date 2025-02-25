namespace NettoRetail.API.Models.Response;

public record ApiResponse(string Message, Dictionary<string, string[]>? Errors = null);

public record ApiResponse<TModel>(TModel Data);
