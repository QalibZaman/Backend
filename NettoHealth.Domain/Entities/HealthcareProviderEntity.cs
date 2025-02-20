using NettoHealth.Domain.Enums;

namespace NettoHealth.Domain.Entities;

public class HealthcareProviderEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public GenderEnum Gender { get; set; }
    public SubscriptionEnum Subscription { get; set; }
    public SpecialityEnum Speciality { get; set; }
}