using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Response
{
    public class SubscriptionPlanResponce
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Motto { get; set; }
        public required int MonthlyPrice { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
