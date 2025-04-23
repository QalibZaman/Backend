namespace NettoRetail.SeederApp.Models
{
    public class AccountSeedModel
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Status { get; set; }
        public required int RoleId { get; set; }
    }
}
