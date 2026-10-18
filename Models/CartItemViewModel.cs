namespace LaptopShop.Models
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice => UnitPrice * Quantity;

        public string ImageUrl
        {
            get
            {
                int[] jpgIds = { 1, 2, 3, 4, 6, 46, 47, 48, 49, 51 };
                if (System.Array.IndexOf(jpgIds, ProductId) >= 0)
                {
                    return $"/images/products/{ProductId}.jpg";
                }
                return $"/images/products/{ProductId}.webp";
            }
        }
    }
}
