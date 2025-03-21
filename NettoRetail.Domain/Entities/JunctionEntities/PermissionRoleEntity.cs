namespace NettoRetail.Domain.Entities.JunctionEntities
{
    public class PermissionRoleEntity
    {
        public int RoleId { get; set; }
        public RoleEntity? Role { get; set; }
        public int PermissionId { get; set; }
        public PermissionEntity? Permission { get; set; }
    }
}
