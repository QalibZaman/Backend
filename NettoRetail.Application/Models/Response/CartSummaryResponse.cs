namespace NettoRetail.Application.Models.Response
{
    public class CartSummaryResponse
    {
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
        public int TotalQuantity { get; set; }
    }
}
