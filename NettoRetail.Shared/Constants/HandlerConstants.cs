namespace NettoRetail.Shared.Constants
{
    public class HandlerConstants
    {
        public static class Customer
        {
            public const string CustomerNotFoundMessage = "Customer {0} not found.";
            public const string CustomerHasActiveOrdersMessage = "Cannot delete the customer with orders in progress, close the orders first.";
            public const string CannotDeleteCustomerFromAnotherShopMessage = "Cannot delete customer from another shop.";
            public const string EmailIsAlreadyUsedMessage = "Account with this email already exists";
            public const string GeneratedLoyaltyIdAlreadyExistMessage = "Generated Loyalty ID already exists.";
        }

        public static class Staff
        {
            public const string StaffNotFoundMessage = "Staff member with ID {0} not found.";
        }

        public static class Order
        {
            public const string OrderNotFoundMessage = "There is no such order.";

            public const string OrderCannotBeEditedMessage = "This order cannot be edited while in 'In Progress' status.";

            public const string InsufficientStockItem = "Insufficient stock for item {0}.";
        }

        public static class Inventory
        {
            public const string InventoryItemNotFoundMessage = "Inventory item {0} not found";
            public const string InventoryItemNotEnoughMessage = "Insufficient stock for item {0}";
            public const string AvailableAmountNotEnoughMessage = "Available amount must be non-negative (item {0}).";
            public const string ReservedAmountNotEnoughMessage = "Reserved amount must be non-negative (item {0}).";
            public const string ReservedAvailableCannotExceedOnHand = "Available + Reserved cannot exceed OnHand (item {0})";
            public const string InventoryItemsNotFoundMessage = "There are no such items.";
        }

        public static class Payment
        {
            public const string PaymentMethodAlreadyExistsMessage = "Payment method already exists.";
            public const string CannotAddPaymentMethodForAnotherShopMessage = "You cannot add a payment method for a shop you do not belong to.";

            public const string PaymentMethodNotFoundMessage = "Payment method not found.";
            public const string PaymentMethodDoesntBelongMessage = "Payment method does not belong to this account.";
            public const string AtLeastTwoPaymentMethodsRequiredMessage = "At least two payment methods are required to change default.";
            public const string PaymentMethodExpiredMessage = "Payment method is expired.";
            public const string AccountNotFoundMessage = "Account {0} not found.";
            public const string AccountIsEmptyMessage = "AccountId {0} is empty.";
            public const string InvalidPaymentMethodIdMessage = "Invalid payment method ID.";
            public const string ErrorOccuredWhenProcessingPaymentMessage = "An error occurred while processing the payment.";
            public const string PaymentMethodCardBrandNotAllowedMessage = "Only mastercard and visa cards are supported.";
        }

        public static class Shop
        {
            public const string ShopNotFoundMessage = "Shop {0} not found";
            public const string AccountAlreadyHasShopMessage = "Account already has a shop.";
            public const string FailedToCreateShopMessage = "Failed to create shop. {0}";
        }

        public static class Account
        {

            public const string AccountNotFoundMessage = "Account {0} not found.";
            public const string PaymentMethodAlreadyExistsMessage = "Payment method already exists.";

            public const string PaymentMethodNotFoundMessage = "Payment method not found.";
            public const string PaymentMethodDoesntBelongMessage = "Payment method does not belong to this account.";
            public const string AtLeastTwoPaymentMethodsRequiredMessage = "At least two payment methods are required to change default.";
            public const string CannotDeleteOwnAccountMessage = "You cannot delete your own account.";
            public const string CannotDeletePlatformAdminMessage = "Only deactivated or pending platform admins can be deleted.";
        }

        public static class Stripe
        {
            public const string StripeCustomerIsNotConfiguredMessage = "Stripe customer is not configured for this account.";
        }

        public static class Subscription
        {
            public const string SubscriptionPlanNotFoundMessage = "Subscription plan not found.";
        }

        public static class Role
        {
            public const string RoleNotFoundMessage = "Role with such Id does not exist.";
        }

        public static class Address
        {
            public const string AddressNotFoundMessage = "Address with such Id does not exist.";
            public const string AtLeastTwoAddressesRequiredMessage = "At least two addresses are required to change the default.";
            public const string AddressAlreadyDefaultMessage = "This address is already a default address.";
        }

        public static class Brand
        {
            public const string BrandNotFoundMessage = "Brand with such Id does not exist.";
            public const string WebsiteUrlUniqueMessage = "Brand's website url must be unique";
        }

        public static class Category
        {
            public const string CategoryNotFoundMessage = "Category with such Id does not exist.";
        }

        public static class Admin 
        {
            public const string CannotEditOwnAccountMessage = "You cannot edit your own account.";
            public const string PlatformAdminNotFoundMessage = "Platform admin with such Id does not exist.";
            public const string NoChangesToSaveMessage = "No changes to save.";
        }
    }
}
