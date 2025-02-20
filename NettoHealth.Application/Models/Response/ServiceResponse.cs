namespace NettoHealth.Application.Models.Response;

public record ServiceResponse(string Message, bool Success);
public record ServiceResponse<T>(string Message, T Data, bool Success = true);