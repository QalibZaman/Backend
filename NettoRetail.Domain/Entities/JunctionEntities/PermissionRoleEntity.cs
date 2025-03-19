namespace NettoRetail.Domain.Entities.JunctionEntities
{
    public class PermissionRoleEntity
    {
        public int RoleId { get; set; }
        public required RoleEntity Role { get; set; }
        public int PermissionId { get; set; }
        public required PermissionEntity Permission { get; set; }

        public bool IsAllowed { get; set; }
    }

}
