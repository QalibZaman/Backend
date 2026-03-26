namespace NettoRetail.Shared.Constants
{
    public class HandlerConstants
    {
        public static class Order
        {
            public const string OrderNotFoundMessage = "There is no such order.";
        }

        public static class Payment
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
