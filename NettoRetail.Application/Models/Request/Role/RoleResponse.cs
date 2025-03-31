namespace NettoRetail.Application.Models.Request.Role;

public class RoleResponse
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required int UserCount { get; set; }
}