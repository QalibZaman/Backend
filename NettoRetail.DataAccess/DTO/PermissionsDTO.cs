using NettoRetail.Domain.Entities.Enums;

namespace NettoRetail.DataAccess.DTO
{
    public class PermissionsDTO
    {
        public int PermissionId { get; set; }
        public AccessLevel AccessLevel { get; set; }
    }
}
