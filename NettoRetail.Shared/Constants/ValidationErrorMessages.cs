namespace NettoRetail.Shared.Constants;

public static class ValidationErrorMessages
{
    public static class Staff
    {
        public const string InvalidLoginErrorMessage = "Invalid email or password.";
        public const string AccountLockedErrorMessage = "Account locked.";
        public const string LockedAccountEmailErrorMessage = "You have entered invalid credentials 3 times in a row. We have sent the instructions on restoring access to your email. Please check you email. If you didn't receive email please check the spam folder or contact support.";
        public const string InvalidPasswordErrorMessage = "Invalid email or password. {0} attempts left.";
    }
}