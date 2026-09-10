using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Request.Role
{
    public class RoleSearchByIdModel
    {
        public int? Id { get; init; }
    }

    public class RoleSearchByNameModel
    {
        public string? Name { get; init; }
    }
}
