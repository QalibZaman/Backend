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

    public static class Role
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 50;
        public const int DescriptionMaxLength = 500;

        public const string CannotDeleteAdminRoleErrorMessage = "The admin role cannot be deleted.";
        public const string CannotEditAdminRoleErrorMessage = "The admin role cannot be edited.";
        public const string RoleInUseErrorMessage = "Cannot delete role. This role is currently assigned to active staff. Please reassign or remove staff before deletion.";
        public const string RoleExistsErrorMessage = "Role with such name already exists in your shop";
        public const string RoleNotFoundErrorMessage = "There is no such role with this Id";
        public const string RoleNotFoundWithShopIdErrorMessage =
            "Role with Id {0} and ShopId {1} does not exist.";

        public const string NameLengthErrorMessage = "Role name length must be more than 2 and  less than 50 characters long.";
        public const string NameAllowedCharactersRegex = "Role name must only contain letters, digits, '-' and '_'.";
        public const string NameAllowedCharactersPattern = @"^[a-zA-Z0-9-_]*$";

        public const string DescriptionMaxLengthErrorMessage = "Description must be less than 500 characters long.";
        public const string DescriptionAllowedCharactersRegex = "Description must only contain letters, digits, spaces, and common punctuation.";
        public const string DescriptionAllowedCharactersPattern = @"^[A-Za-z0-9 ,.\-_'()]*$";
        public const string DescriptionTrimErrorMessage = "Description must not start or end with spaces.";

        public const string PermissionsDontExistErrorMessage = "Some of the permissions don't exist. Please provide valid permissions.";
        public const string PermissionListUniqueIdsErrorMessage = "Permissions must be unique.";
        public const string PermissionsListNullErrorMessage = "Permissions list cannot be null";
        public const string PermissionsListEmptyErrorMessage = "At least one permission must be enabled";

        public const string CannotAssignSystemPermissionErrorMessage = "System permissions cannot be assigned";

        public const string PermissionNotFound = "Permission not found";
        public const string AccessLevelNotFound = "Access level not found";
    }

    public static class ShopDocument
    {
        public const string InvalidFormatOrSize = "Please upload a PDF, DOCX, or ODT file under 20MB";
    }

    public static class Shop
    {
        public const string ShopDoesntExistErrorMessage = "No shop with Id: {0}";
        public const string ShopIdRequiredErrorMessage = "ShopId is required.";
        public const string InvalidUrlErrorMessage =
            "URL is invalid";
    }
}