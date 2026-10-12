namespace NettoRetail.Shared.Constants;

public static class StaffConstants
{
    public const string StaffMemberNotFound = "Staff member not found.";
    public const string StaffMemberAccessDenied = "You are not allowed to access this staff member.";

    public const string AccountDeletedNotification =
        "Your account has been removed. Your session has been terminated for security reasons. You will be redirected to the Login page in 10 seconds.";

    public const string AccountLocked = "Account locked. Try again later.";
    public const string IncorrectPassword = "Current password is incorrect";

    public const int DeleteAccountLockoutMinutes = 15;
}