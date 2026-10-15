using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Request.Notification
{
    public class CreateNotificationRequest
    {
        public required Guid StaffId { get; set; }
        public required string Topic { get; set; }
        public required string Message { get; set; }
    }
}
