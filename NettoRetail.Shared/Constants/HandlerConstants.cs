namespace NettoRetail.Shared.Constants
{
    public class HandlerConstants
    {
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

            public const string PaymentMethodNotFoundMessage = "Payment method not found.";
            public const string PaymentMethodDoesntBelongMessage = "Payment method does not belong to this account.";
            public const string AtLeastTwoPaymentMethodsRequiredMessage = "At least two payment methods are required to change default.";
        }

        public static class Shop
        {
            public const string ShopNotFoundMessage = "Shop {0} not found";
        }

        public static class Account
        {

            public const string AccountNotFoundMessage = "Account {0} not found.";
            public const string PaymentMethodAlreadyExistsMessage = "Payment method already exists.";

            public const string PaymentMethodNotFoundMessage = "Payment method not found.";
            public const string PaymentMethodDoesntBelongMessage = "Payment method does not belong to this account.";
            public const string AtLeastTwoPaymentMethodsRequiredMessage = "At least two payment methods are required to change default.";
        }

        public static class Stripe
        {
            public const string StripeCustomerIsNotConfiguredMessage = "Stripe customer is not configured for this account.";
        }
    }
}
