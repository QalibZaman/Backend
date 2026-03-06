namespace NettoRetail.Application.Models.Request.Role;

public class RoleSearchModel : PaginationRequest
{
    public string? Name { get; init; }
}