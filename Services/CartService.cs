using System.Text.Json;
using LaptopShop.Data;

using LaptopShop.Models;
using Microsoft.AspNetCore.Http;

namespace LaptopShop.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private const string CartKey = "Cart";

        public CartService(
       IHttpContextAccessor httpContextAccessor,
       ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        private ISession Session
        {
            get
            {
                var context = _httpContextAccessor.HttpContext
                    ?? throw new InvalidOperationException(
                        "Không có yêu cầu HTTP hiện tại.");

                return context.Session;
            }
        }

        public List<CartItem> GetCart()
        {
            string? json = Session.GetString(CartKey);

            if (string.IsNullOrEmpty(json))
            {
                return new List<CartItem>();
            }

            return JsonSerializer.Deserialize<List<CartItem>>(json)
                ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            string json = JsonSerializer.Serialize(cart);

            Session.SetString(CartKey, json);
        }
        public void AddItem(int productId, int quantity)
        {
            // 1. Kiểm tra số lượng muốn thêm
            if (quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Số lượng thêm phải lớn hơn 0.");
            }

            // 2. Tìm sản phẩm trong database
            var product = _context.Products.Find(productId);

            if (product == null)
            {
                throw new InvalidOperationException(
                    "Sản phẩm không tồn tại.");
            }

            // 3. Đọc giỏ và tìm dòng sản phẩm đã có
            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(
                item => item.ProductId == productId);

            int currentQuantity = existingItem?.Quantity ?? 0;

            // 4. Kiểm tra số lượng sau khi thêm
            // Dùng long để tránh tràn số khi cộng hai số int
            long newQuantity = (long)currentQuantity + quantity;

            if (newQuantity > product.StockQuantity)
            {
                throw new InvalidOperationException(
                    $"Sản phẩm chỉ còn {product.StockQuantity} chiếc trong kho.");
            }

            // 5. Thêm dòng mới hoặc cập nhật dòng hiện có
            if (existingItem == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = (int)newQuantity
                });
            }
            else
            {
                existingItem.Quantity = (int)newQuantity;
            }

            // 6. Lưu giỏ đã thay đổi vào Session
            SaveCart(cart);
        }
        //Xoa di ca mot san pham
        public void RemoveItem(int productId)
        {
           
            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(
                item => item.ProductId == productId);
            if(existingItem == null)
            {
                return;

            }
            cart.Remove(existingItem);
            SaveCart(cart);

        }
        //xoa di mot luong san pham
        public void DecreaseItem(int productId)
        {
            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(
                item => item.ProductId == productId);
            if(existingItem == null)
            {
                throw new InvalidOperationException(
                    "san pham khong co o gio");
            }
            if(existingItem.Quantity > 1)
            {
                existingItem.Quantity -= 1;
            }
            else 
            {
                cart.Remove(existingItem);

            }
            SaveCart(cart);
            
        }

        public List<CartItemViewModel> GetCartDetails()
        {
            var cart = GetCart();

            var productIds = cart
                .Select(item => item.ProductId)
                .ToList();

            var products = _context.Products
                .Where(product => productIds.Contains(product.Id))
                .ToList();

            var result = new List<CartItemViewModel>();

            foreach (var item in cart)
            {
                var product = products.FirstOrDefault(
                    product => product.Id == item.ProductId);

                if (product == null)
                {
                    continue;
                }

                result.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                });
            }

            return result;
        }
        public List<CartItemViewModel> GetCheckoutItems()
        {
            var cart = GetCart();

            if (cart.Count == 0)
            {
                throw new InvalidOperationException("Giỏ hàng đang trống.");
            }

            var productIds = cart
                .Select(item => item.ProductId)
                .ToList();

            var products = _context.Products
                .Where(product => productIds.Contains(product.Id))
                .ToList();

            var result = new List<CartItemViewModel>();

            foreach (var item in cart)
            {
                var product = products.FirstOrDefault(
                    product => product.Id == item.ProductId);

                if (product == null)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm mã {item.ProductId} không còn tồn tại.");
                }

                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        $"Số lượng của {product.Name} không hợp lệ.");
                }

                if (item.Quantity > product.StockQuantity)
                {
                    throw new InvalidOperationException(
                        $"{product.Name} chỉ còn {product.StockQuantity} chiếc.");
                }

                result.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                });
            }

            return result;
        }
        public void ClearCart()
        {
            Session.Remove(CartKey);
        }


    }
}
