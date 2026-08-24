namespace NettoRetail.Shared.Constants;

public static class ValidationErrorMessages
{
    public static class Staff
    {
        public const string FirstNameIsInvalidErrorMessage = "Invalid First Name. Please check format or length.";
        public const string LastNameIsInvalidErrorMessage = "Invalid Last Name. Please check format or length.";
        public const string EmailIsInvalidErrorMessage = "Invalid Email. Please check format or length.";
        public const string PasswordIsInvalidErrorMessage = "Password must contain between 8-20 characters, include at least one uppercase letter, one lowercase letter, one number and one special character.";
        public const string PasswordIsRequiredErrorMessage = "Password is required.";
    }
}