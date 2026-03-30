namespace NettoRetail.Shared.Constants;

public static class ValidationErrorMessages
{
    public const string ANY_PROP_UPDATE_ERROR_MESSAGE = "At least one property for update must be supplied.";

    public static class Inventory
    {
        public const int InvalidThreshold = 0;
        public const int DescriptionMaxLength = 500;
        public const int ProductNameMinLength = 2;
        public const int ProductNameMaxLength = 50;
        public const string IsRequired = "Product name is required";
        public const string NameMinLength = "Product name length must be greater than {MinLength} characters long.";
        public const string NameMaxLength = "Product name length must be less than {MaxLength} characters long.";
        public const string ProductNameRegex = @"^[a-zA-Z0-9_\- ]+$";
        public const string ProductNameAllowedCharactersRegex = "Product name must only contain letters, digits, spaces, '-' and '_'.";
        public const string UpcNotEmpty = "Product UPC is required.";
        public const string UpcCodeRegex = @"^\d{12}$";
        public const string UpcMinLength = "Product UPC must be exactly 12 digits long.";
        public const string InvalidPriceScaleMessage = "Price must not have more than 2 decimal places.";
        public const string DescriptionRegex = @"^[-a-zA-Z0-9.,!?;:'""()-]+( [a-zA-Z0-9.,!?;:'""()-]+)*$";
        public const string DescriptionInvalidFormatMessage = "Description must only contain letters, digits, and common punctuation, and cannot start or end with a space.";
        public const string PriceMinlength = "Price must be greater or equal to {ComparisonValue}.";
        public const string DescriptionMaxLengthMessage = "Description must be less than {MaxLength} characters long.";


        public const int FinancialPrecision = 18;
        public const int FinancialScale = 2;
    }

    public static class Search
    {
        public const string InvalidSearchField = "Invalid search field.";
        public const string SearchTermRequired = "Search term is required.";
        public const string SearchTermMaxLength = "Search term must be less than 100 characters.";
        public const string InvalidCharacters = "Invalid characters.";
        public const string InvalidNumericValue = "Invalid numeric value.";
        public const string InvalidEmailFormat = "Invalid email format.";
    }

    public static class Content
    {
        public const int MaxContentLength = 50 * 1024; // 50 KB
        public const string IsRequired = "Content is required.";
        public const string MaxContentLengthMessage = "Content cannot exceed {MaxLength} characters.";
        public const string ContentVisibilityRequiredMessage = "Visibility must be false when content is empty or whitespace.";
    }

    public static class Role
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 50;
        public const int DescriptionMaxLength = 500;
        public const string NameLengthErrorMessage = "Role name length must be more than 2 and  less than 50 characters long.";
        public const string NameAllowedCharactersRegex = "Role name must only contain letters, digits, '-' and '_'.";
        public const string NameAllowedCharactersPattern = @"^[a-zA-Z0-9-_]*$";
        public const string DescriptionMaxLengthErrorMessage = "Description must be less than 500 characters long.";
        public const string DescriptionAllowedCharactersRegex = "Description must only contain letters, digits, spaces, and common punctuation.";
        public const string DescriptionAllowedCharactersPattern = @"^[A-Za-z0-9 ,.\-_'()]*$";
        public const string DescriptionTrimErrorMessage = "Description must not start or end with spaces.";
        public const string PermissionsListNullErrorMessage = "Permissions list cannot be null";
        public const string PermissionsListEmptyErrorMessage = "At least one permission must be enabled";

        public const string RoleExistsErrorMessage = "Role with such name already exists in your shop";
        public const string ShopDoesntExistErrorMessage = "No shop with Id: {0}";
        public const string ShopIdRequiredErrorMessage = "ShopId is required.";
        public const string PermissionsDontExistErrorMessage = "Some of the permissions don't exist. Please provide valid permissions.";
        public const string PermissionListUniqueIdsErrorMessage = "Permission IDs must be unique.";
    }

    public static class Customer
    {
        public const string CustomerNotFoundErrorMessage = "There is no such customer.";
    }

    public static class Order
    {
        public const string ItemGreaterThanZeroErrorMessage = "Item quantity must be greater than 0.";
        public const string ItemUpsertDeleteConflictErrorMessage = "Items cannot be both upserted and deleted";
        public const string InvalidDelivery = "The selected delivery type is invalid";
    }

    public static class Payment
    {
        public const string IdNotEmpty = "AccountId cannot be an empty GUID";
        public const string TokenNotEmpty = "Token cannot be empty";
        public const string InvalidPaymentMethodIdFormat = "Invalid payment method ID format";
    }
}