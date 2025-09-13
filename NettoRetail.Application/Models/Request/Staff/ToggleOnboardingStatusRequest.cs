namespace NettoRetail.Application.Models.Request.Staff;

public class ToggleOnboardingStatusRequest
{
    public required bool IsOnboardingCompleted { get; set; }
}