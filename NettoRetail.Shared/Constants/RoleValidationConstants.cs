namespace NettoRetail.Shared.Constants;

public static class RoleValidationConstants
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 50;
    public const int DescriptionMaxLength = 500;

    public const string NameRegex = "^[A-Za-z0-9_-]+$";
    public const string DescriptionRegex = @"^[A-Za-z0-9 ,.\-_'()]*$";
}
