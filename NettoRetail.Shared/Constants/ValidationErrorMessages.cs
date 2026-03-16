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
}