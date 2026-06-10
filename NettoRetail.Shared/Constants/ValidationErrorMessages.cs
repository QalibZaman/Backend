namespace NettoRetail.Shared.Constants;

public static class ValidationErrorMessages
{
    public const string ANY_PROP_UPDATE_ERROR_MESSAGE = "At least one property for update must be supplied.";

    public static class Common
    {
        public const string InvalidDateRangeErrorMessage = "Start date must be before or equal to end date.";
    }

    public static class Inventory
    {
        public const int InvalidThreshold = 0;
        public const int DescriptionMaxLength = 500;
        public const int ProductNameMinLength = 2;
        public const int ProductNameMaxLength = 50;

        public const string ProductNameRegex = @"^[a-zA-Z0-9_\- ]+$";
        public const string UpcCodeRegex = @"^\d{12}$";
        public const string ProductLocationRegex = @"^(?! )[a-zA-Z0-9,\- ]*(?<! )$";

        public const string ItemNotFoundErrorMessage = "There is no such item with this Id";
        public const string IsRequired = "Product name is required";
        public const string NameMinLength = "Product name length must be greater than {MinLength} characters long.";
        public const string NameMaxLength = "Product name length must be less than {MaxLength} characters long.";
        public const string ProductNameAllowedCharactersRegex = "Product name must only contain letters, digits, spaces, '-' and '_'.";
        public const string UpcNotEmpty = "Product UPC is required.";
        public const string UpcMinLength = "Product UPC must be exactly 12 digits long.";
        public const string UpcNotEmptyErrorMessage = UpcNotEmpty;
        public const string UpcMinLengthErrorMessage = UpcMinLength;
        public const string InvalidPriceScaleMessage = "Price must not have more than 2 decimal places.";
        public const string MaxPriceMaxLengthErrorMessage = "Max price must be greater than or equal to min price.";
        public const string DescriptionRegex = @"^[-a-zA-Z0-9.,!?;:'""()-]+( [a-zA-Z0-9.,!?;:'""()-]+)*$";
        public const string DescriptionInvalidFormatMessage = "Description must only contain letters, digits, and common punctuation, and cannot start or end with a space.";
        public const string ProductLocationInvalidFormatMessage = "Product location must only contain letters, digits, spaces, hyphens, and commas, and cannot start or end with a space.";
        public const string PriceMinlength = "Price must be greater or equal to {ComparisonValue}.";
        public const string DescriptionMaxLengthMessage = "Description must be less than {MaxLength} characters long.";
        public const string InvalidStatusErrorMessage = "Invalid status value provided.";
        public const string MaxOnHandAmountMinLengthMessage = "Max on-hand amount must be greater than or equal to min on-hand amount.";
        public const string MaxReservedAmountMinLengthMessage = "Max reserved amount must be greater than or equal to min reserved amount.";
        public const string MaxAvailableAmountMinLengthMessage = "Max available amount must be greater than or equal to min available amount.";

        public const int FinancialPrecision = 18;
        public const int FinancialScale = 2;
    }

    public static class PasswordValidation
    {
        public const string PasswordSize = "Password must contain at least 8 characters";
    }
    

    public static class Password
    {
        public const string PasswordSize = "Password must contain at least 8 characters";
        public const string PasswordMaxSize = "Password must be less than 20 characters long.";
        public const string PasswordConfirmationMismatchErrorMessage = "New password and confirmation password do not match. Please check your input.";
        public const string CurrentPasswordIncorrectErrorMessage = "The current password you entered is incorrect.";
        public const string NewPasswordMatchesCurrentErrorMessage = "New password cannot match the current one.";
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
        public const int MaxContentLength = 50000;
        public const string IsRequired = "Content is required.";
        public const string MaxContentLengthMessage = "Content cannot exceed {MaxLength} characters.";
        public const string ContentVisibilityRequiredMessage = "Visibility must be false when content is empty or whitespace.";
        public const string ImageLimitExceededMessage = "Image limits exceeded. Please remove excess images or check file sizes/formats.";
        public const string InvalidImageFormatMessage = "Invalid file format.";
        public const string ImageSizeTooSmallMessage = "File size must be at least 1KB.";
        public const string ImageSizeTooLargeMessage = "File size exceeds 5MB limit.";
    }

    public static class Role
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 50;
        public const int DescriptionMaxLength = 500;

        public const string CannotDeleteAdminRoleErrorMessage = "The admin role cannot be deleted.";
        public const string RoleInUseErrorMessage = "Cannot delete role. This role is currently assigned to active staff. Please reassign or remove staff before deletion.";
        public const string RoleExistsErrorMessage = "Role with such name already exists in your shop";
        public const string RoleNotFoundErrorMessage = "There is no such role with this Id";

        public const string NameLengthErrorMessage = "Role name length must be more than 2 and  less than 50 characters long.";
        public const string NameAllowedCharactersRegex = "Role name must only contain letters, digits, '-' and '_'.";
        public const string NameAllowedCharactersPattern = @"^[a-zA-Z0-9-_]*$";

        public const string DescriptionMaxLengthErrorMessage = "Description must be less than 500 characters long.";
        public const string DescriptionAllowedCharactersRegex = "Description must only contain letters, digits, spaces, and common punctuation.";
        public const string DescriptionAllowedCharactersPattern = @"^[A-Za-z0-9 ,.\-_'()]*$";
        public const string DescriptionTrimErrorMessage = "Description must not start or end with spaces.";

        public const string PermissionsDontExistErrorMessage = "Some of the permissions don't exist. Please provide valid permissions.";
        public const string PermissionListUniqueIdsErrorMessage = "Permission IDs must be unique.";
        public const string PermissionsListNullErrorMessage = "Permissions list cannot be null";
        public const string PermissionsListEmptyErrorMessage = "At least one permission must be enabled";
    }

    public static class Shop
    {
        public const string ShopDoesntExistErrorMessage = "No shop with Id: {0}";
        public const string ShopIdRequiredErrorMessage = "ShopId is required.";
    }

    public static class Customer
    {
        public const string CustomerNotFoundErrorMessage = "There is no such customer.";
        public const string CannotEditDeactivatedCustomerErrorMessage = "You cannot edit a deactivated customer unless you are reactivating them";
        public const string CustomerHasOrdersErrorMessage = "This customer has active orders. Please complete or cancel them before deactivating";
    }

    public static class Order
    {
        public const string ItemGreaterThanZeroErrorMessage = "Item quantity must be greater than 0.";
        public const string ItemUpsertDeleteConflictErrorMessage = "Items cannot be both upserted and deleted";
        public const string InvalidDelivery = InvalidDeliveryErrorMessage;
        public const string InvalidDeliveryErrorMessage = "The selected delivery type is invalid";
        public const string InvalidOrderStatusErrorMessage = "The selected order status is invalid";
    }

    public static class Payment
    {
        public const string IdNotEmpty = "AccountId cannot be an empty GUID";
        public const string TokenNotEmpty = "Token cannot be empty";
        public const string InvalidPaymentMethodIdFormat = "Invalid payment method ID format";
    }

    public static class Subscription
    {
        public const string TrimmedAlphanumericRegex = @"^[A-Za-z0-9_-](?:[A-Za-z0-9 _-]*[A-Za-z0-9_-])?$";
        public const string ConsecutiveSpecialCharsRegex = @"(--|__|-_|_-)";

        public const string NameFormatErrorMessage = "The name can only contain letters, numbers, hyphens, underscores and not start or end with a space. Please check your input.";
        public const string NameConsecutiveSpecialCharsErrorMessage = "Name must not contain consecutive special characters: --, __, -_, or _-.";
        public const string PriceRequiredErrorMessage = "\"Field price\" cannot be empty.";
        public const string DescriptionFormatErrorMessage = "The description can only contain letters, numbers, hyphens, underscores and not start or end with a space. Please check your input.";
        public const string MottoFormatErrorMessage = "The motto can only contain letters, numbers, hyphens, underscores and not start or end with a space. Please check your input.";
        public const string FeatureIdsRequiredErrorMessage = "At least one feature must be enabled.";
        public const string FeatureIdsPositiveErrorMessage = "Feature IDs must be positive integers.";
        public const string FeatureIdsUniqueErrorMessage = "Feature IDs must be unique.";
    }

    public static class Staff
    {
        public const string InvalidFirstNameErrorMessage = "Invalid First Name. Please check format or length.";
        public const string InvalidLastNameErrorMessage = "Invalid Last Name. Please check format or length.";
        public const string InvalidStatusErrorMessage = "Invalid status value provided.";
        public const string AtLeastOneFieldErrorMessage = "At least one field must be provided to update.";
        public const string ActivationStatusRequired = "Activation status must be provided when updating staff.";
    }

    public static class Account
    {
        public const string AccountNotFoundErrorMessage = "There is no such account.";
    }

    public static class Sort
    {
        public const string InvalidSortFieldErrorMessage = "Invalid sort field provided.";
        public const string SortingFieldRequiredMessage = "Sorting field is required.";
    }

    public static class StaffCsv
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 30;
        public const int EmailMaxLength = 256;

        public const string NamePattern = @"^([A-Z][a-z]*)( [A-Z][a-z]*)*$";
        public const string EmailPattern = @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$";

        public const string FirstNameRequired = "First name is required.";
        public const string FirstNameMinLength = "First name must be at least 2 characters.";
        public const string FirstNameMaxLength = "First name must be no more than 30 characters.";
        public const string FirstNameInvalidFormat = "First name must start with a capital letter and only contain Latin letters. Spaces are allowed between words.";

        public const string LastNameRequired = "Last name is required.";
        public const string LastNameMinLength = "Last name must be at least 2 characters.";
        public const string LastNameMaxLength = "Last name must be no more than 30 characters.";
        public const string LastNameInvalidFormat = "Last name must start with a capital letter and only contain Latin letters. Spaces are allowed between words.";

        public const string EmailRequired = "Email is required.";
        public const string EmailMaxLengthMessage = "Email must be no more than 256 characters.";
        public const string EmailInvalidFormat = "Invalid email format. Must be a valid email like name@domain.com.";

        public const string RoleRequired = "Role is required.";
        public const string RoleInvalid = "Role must be a valid role id.";
    }

    public static class Brand
    {
        public const string InvalidBrandNameErrorMessage = "Brand name must only contain letters, digits, spaces, '-' and '_'.";
        public const string InvalidBrandDescriptionErrorMessage = "Brand description contains invalid characters";
    }

    public static class Pagination
    {
        public const string PaginationTooLargeErrorMessage = "Pagination parameters are too large.";
    }

    public static class Name
    {
        public const string TooShortErrorMessage = "Must be at least {0} characters.";
        public const string TooLongErrorMessage = "Must be no more than {0} characters.";
        public const string PatternErrorMessage = "Must start with a capital letter and contain only Latin letters. Spaces are allowed between words.";
    }

    public static class Email
    {
        public const string TooLongErrorMessage = "Email must be no more than {0} characters.";
        public const string PatternErrorMessage = "Must be a valid email address like name@domain.com.";
        public const string AlreadyExistsErrorMessage = "Another account with this email is already exists";
    }

    public static class Country
    {
        public const string CountryNotFoundErrorMessage = "There is no such country.";
        public const string InvalidZipCodeErrorMessage = "Invalid zip code format for the specified country.";
    }

    public static class Address
    {
        public const string MaxAddressesLimitErrorMessage = "You cannot have more than 10 addresses.";
        public const string InvalidCityFormatErrorMessage = "City must start with an uppercase letter and contain only letters, spaces, or hyphens.";
        public const string InvalidStreetFormatErrorMessage = "Street must start with an uppercase letter and contain valid address characters.";
        public const string InvalidBuildingNumberFormatErrorMessage = "Building number can only contain letters, numbers, spaces, hyphens, or slashes.";
        public const string OfficeNumberAndApartmentNumberConflictErrorMessage = "Cannot provide both Office and Apartment numbers.";
        public const string InvalidAddressFormatErrorMessage = "The address contains invalid characters or restricted spaces.";
        public const string InvalidCombinedAddressFormatErrorMessage = "The combined address (Street, Building, Floor, Entrance, Office/Apartment) must be between 5 and 90 characters long and contain only valid characters.";
    }

    public static class Delivery
    {
        public const string MethodSelectionObligatory = "Courier delivery method must be selected before setting an address.";
    }

    public static class Website
    {
        public const string InvalidWebsiteUrlErrorMessage = "Website URL must be a valid URL including protocol (e.g., https://).";
    }

    public static class File
    {
        public const string InvalidFileSizeErrorMessage = "File size must be 5MB or less.";
        public const string UnsupportedFileFormatErrorMessage = "Unsupported file format. Only JPG, JPEG or PNG allowed.";
    }
}
