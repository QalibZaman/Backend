using NettoHealth.Domain.Enums;

namespace NettoHealth.Application.Models.Entities;

public class HealthcareProviderRegistrationModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public GenderEnum Gender { get; set; }
    public SubscriptionEnum Subscription { get; set; }
    public SpecialityEnum Speciality { get; set; }
}