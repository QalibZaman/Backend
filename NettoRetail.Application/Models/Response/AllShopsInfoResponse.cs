using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response
{
    public class AllShopsInfoResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string AdminFullName { get; set; }
        public required string AdminEmail { get; set; }
        public required string AdminPhoneNumber { get; set; }
        public required string Subscription { get; set; }
    }
}
