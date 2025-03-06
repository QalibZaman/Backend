using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Domain.Entities;

public class LoginAttemptEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StaffId { get; set; }

    public int FailedAttempts { get; set; } = 0;
    public DateTime? LockoutEndTime { get; set; }
    public DateTime LastFailedLoginTime { get; set; } = DateTime.UtcNow;

    public StaffEntity Staff { get; set; } = null!;
}
