namespace NettoRetail.Shared.Constants;

public class HandlerConstants
{
    public static class Admin
    {
        public const string AccountAlreadyExistsMessage = "Account with this email already exists";
    }

    public static class Payment
    {
        public const string PaymentMethodAlreadyExistsMessage = "The card is already added";
        public const string AccountNotFoundMessage = "Account with id {0} was not found";
        public const string ShopNotFoundMessage = "Shop with id {0} was not found";
        public const string CannotAddPaymentMethodForAnotherShopMessage = "Cannot add a payment method for another shop";
    }
}