using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Admin
{
	public class CreatePlatformAdminModel
	{
		public string Email { get; set; } = default!;
		public string FirstName { get; set; } = default!;
		public string LastName { get; set; } = default!;
		public string? PhoneNumber { get; }
		public List<PlatformAdminPermission> Permissions { get; }

		public CreatePlatformAdminModel(CreatePlatformAdminRequest request)
		{
			FirstName = request.FirstName;
			LastName = request.LastName;
			Email = request.Email;
			PhoneNumber = request.PhoneNumber;
			Permissions = request.Permissions;
		}
		public void Normalize()
		{
			Email = Email.Trim().ToLowerInvariant();
			FirstName = FirstName.Trim();
			LastName = LastName.Trim();
		}
	}
}
