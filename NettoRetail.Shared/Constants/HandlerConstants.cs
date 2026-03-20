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
        }
    }
}
