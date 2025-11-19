namespace NettoRetail.Application.Models.Request.Role
{
    public class CreateRoleResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int? ShopId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }
}
