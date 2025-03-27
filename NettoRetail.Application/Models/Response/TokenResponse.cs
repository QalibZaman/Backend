namespace NettoRetail.Application.Models.Response;

public record TokenResponse(string AccessToken, string RefreshToken, string TempToken);
