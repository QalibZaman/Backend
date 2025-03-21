using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class RoleEntity
{
    public int Id { get; set; }
    public RoleEnum RoleName { get; set; }

    public ICollection<AccountEntity> Accounts { get; set; } = null!;
}