using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities
{
    public class PlatformAdminEntity
    {
        public required Guid AccountId { get; set; }
        public AccountEntity Account { get; set; }
        public PlatformAdminStatusEnum PlatformAdminStatus { get; set; } = PlatformAdminStatusEnum.Active;
    }
}
