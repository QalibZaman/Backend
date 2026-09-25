namespace NettoRetail.Shared.Constants;

public static class ValidationErrorMessages
{
    public static class User
    {
        public const string ProfileImageDoesNotExistErrorMessage = "Profile image does not exist.";
        public const string UserNotFoundErrorMessage = "User not found.";
    }

    public static class Staff
    {
        public const string FirstNameIsInvalidErrorMessage =
            "Invalid First Name. Please check format or length.";

        public const string LastNameIsInvalidErrorMessage =
            "Invalid Last Name. Please check format or length.";

        public const string EmailIsInvalidErrorMessage =
            "Invalid Email. Please check format or length.";

        public const string PasswordIsInvalidErrorMessage =
            "Password must contain between 8-20 characters, include at least one uppercase letter, one lowercase letter, one number and one special character.";

        public const string PasswordIsRequiredErrorMessage =
            "Password is required.";

        public const string InvalidLoginErrorMessage =
            "Invalid email or password.";

        public const string AccountLockedErrorMessage =
            "Account locked.";

        public const string LockedAccountEmailErrorMessage =
            "You have entered invalid credentials 3 times in a row. We have sent the instructions on restoring access to your email. Please check you email. If you didn't receive email please check the spam folder or contact support.";

        public const string InvalidPasswordErrorMessage =
            "Invalid email or password. {0} attempts left.";
    }

    public static class Shop
    {
        public const string InvalidUrlErrorMessage =
            "URL is invalid";
    }
}