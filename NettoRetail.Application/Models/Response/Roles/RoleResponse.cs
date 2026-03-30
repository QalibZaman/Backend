namespace NettoRetail.Application.Models.Response.Roles;

public class RoleResponse
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required int UserCount { get; set; }
    public required bool IsEditable { get; set; }
    public required bool IsDeletable { get; set; }
}
