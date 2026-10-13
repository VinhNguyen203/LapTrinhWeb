namespace LaptopShop.Models
{
    public class CartItem
    {
        internal readonly object? ProductName;

        public int ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
