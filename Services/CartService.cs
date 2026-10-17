using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LaptopShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private const string CartKey = "Cart";

        public CartService(
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        private int? CurrentUserId
        {
            get
            {
                var userIdStr = Session?.GetString("UserId");
                if (int.TryParse(userIdStr, out int userId) && userId > 0)
                {
                    return userId;
                }
                return null;
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm trong giỏ (nếu đã đăng nhập thì lấy từ Database, nếu khách vãng lai thì lấy từ Session)
        /// Tự động đồng bộ giỏ hàng từ Session vào Database ngay khi khách hàng đăng nhập.
        /// </summary>
        public List<CartItem> GetCart()
        {
            int? userId = CurrentUserId;

            if (userId.HasValue)
            {
                // Đồng bộ sản phẩm từ Session nếu khách đã thêm hàng trước khi đăng nhập
                SyncSessionCartToDatabase(userId.Value);

                return _context.CartItems
                    .Where(c => c.UserId == userId.Value)
                    .ToList();
            }

            // Khách chưa đăng nhập: Lấy từ Session
            if (Session == null) return new List<CartItem>();

            string? json = Session.GetString(CartKey);
            if (string.IsNullOrEmpty(json))
            {
                return new List<CartItem>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        private void SaveSessionCart(List<CartItem> cart)
        {
            if (Session != null)
            {
                string json = JsonSerializer.Serialize(cart);
                Session.SetString(CartKey, json);
            }
        }

        private void SyncSessionCartToDatabase(int userId)
        {
            if (Session == null) return;

            string? json = Session.GetString(CartKey);
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var sessionCart = JsonSerializer.Deserialize<List<CartItem>>(json);
                if (sessionCart != null && sessionCart.Any())
                {
                    foreach (var sItem in sessionCart)
                    {
                        var dbItem = _context.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == sItem.ProductId);
                        if (dbItem != null)
                        {
                            dbItem.Quantity += sItem.Quantity;
                            dbItem.UpdatedAt = DateTime.Now;
                        }
                        else
                        {
                            _context.CartItems.Add(new CartItem
                            {
                                UserId = userId,
                                ProductId = sItem.ProductId,
                                Quantity = sItem.Quantity,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            });
                        }
                    }
                    _context.SaveChanges();
                }
            }
            catch
            {
                // Bỏ qua lỗi deserialize nếu session cũ không khớp
            }
            finally
            {
                Session.Remove(CartKey);
            }
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng (lưu vào Database nếu đã đăng nhập)
        /// </summary>
        public void AddItem(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                throw new InvalidOperationException("Số lượng thêm phải lớn hơn 0.");
            }

            var product = _context.Products.Find(productId);
            if (product == null)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại.");
            }

            int? userId = CurrentUserId;

            if (userId.HasValue)
            {
                var existingItem = _context.CartItems.FirstOrDefault(c => c.UserId == userId.Value && c.ProductId == productId);
                int currentQty = existingItem?.Quantity ?? 0;
                long newQty = (long)currentQty + quantity;

                if (newQty > product.StockQuantity)
                {
                    throw new InvalidOperationException($"Sản phẩm chỉ còn {product.StockQuantity} chiếc trong kho.");
                }

                if (existingItem == null)
                {
                    _context.CartItems.Add(new CartItem
                    {
                        UserId = userId.Value,
                        ProductId = productId,
                        Quantity = (int)newQty,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
                else
                {
                    existingItem.Quantity = (int)newQty;
                    existingItem.UpdatedAt = DateTime.Now;
                }

                _context.SaveChanges();
                return;
            }

            // Khách chưa đăng nhập: Lưu Session
            var cart = GetCart();
            var sessionItem = cart.FirstOrDefault(item => item.ProductId == productId);
            int curQty = sessionItem?.Quantity ?? 0;
            long totalNewQty = (long)curQty + quantity;

            if (totalNewQty > product.StockQuantity)
            {
                throw new InvalidOperationException($"Sản phẩm chỉ còn {product.StockQuantity} chiếc trong kho.");
            }

            if (sessionItem == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = (int)totalNewQty,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                sessionItem.Quantity = (int)totalNewQty;
                sessionItem.UpdatedAt = DateTime.Now;
            }

            SaveSessionCart(cart);
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        public void RemoveItem(int productId)
        {
            int? userId = CurrentUserId;

            if (userId.HasValue)
            {
                var dbItem = _context.CartItems.FirstOrDefault(c => c.UserId == userId.Value && c.ProductId == productId);
                if (dbItem != null)
                {
                    _context.CartItems.Remove(dbItem);
                    _context.SaveChanges();
                }
                return;
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);
            if (existingItem == null) return;

            cart.Remove(existingItem);
            SaveSessionCart(cart);
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        public void UpdateItem(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                RemoveItem(productId);
                return;
            }

            var product = _context.Products.Find(productId);
            if (product == null)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại.");
            }

            if (quantity > product.StockQuantity)
            {
                throw new InvalidOperationException($"Chỉ còn {product.StockQuantity} sản phẩm trong kho.");
            }

            int? userId = CurrentUserId;

            if (userId.HasValue)
            {
                var dbItem = _context.CartItems.FirstOrDefault(c => c.UserId == userId.Value && c.ProductId == productId);
                if (dbItem != null)
                {
                    dbItem.Quantity = quantity;
                    dbItem.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                }
                return;
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                existingItem.UpdatedAt = DateTime.Now;
                SaveSessionCart(cart);
            }
        }

        /// <summary>
        /// Giảm số lượng 1 sản phẩm trong giỏ hàng
        /// </summary>
        public void DecreaseItem(int productId)
        {
            int? userId = CurrentUserId;

            if (userId.HasValue)
            {
                var dbItem = _context.CartItems.FirstOrDefault(c => c.UserId == userId.Value && c.ProductId == productId);
                if (dbItem == null)
                {
                    throw new InvalidOperationException("Sản phẩm không có trong giỏ hàng.");
                }

                if (dbItem.Quantity > 1)
                {
                    dbItem.Quantity -= 1;
                    dbItem.UpdatedAt = DateTime.Now;
                }
                else
                {
                    _context.CartItems.Remove(dbItem);
                }

                _context.SaveChanges();
                return;
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);
            if (existingItem == null)
            {
                throw new InvalidOperationException("Sản phẩm không có trong giỏ hàng.");
            }

            if (existingItem.Quantity > 1)
            {
                existingItem.Quantity -= 1;
                existingItem.UpdatedAt = DateTime.Now;
            }
            else
            {
                cart.Remove(existingItem);
            }
            SaveSessionCart(cart);
        }

        /// <summary>
        /// Lấy chi tiết thông tin các sản phẩm trong giỏ hàng để hiển thị
        /// </summary>
        public List<CartItemViewModel> GetCartDetails()
        {
            var cart = GetCart();
            var productIds = cart.Select(item => item.ProductId).ToList();
            var products = _context.Products.Where(product => productIds.Contains(product.Id)).ToList();

            var result = new List<CartItemViewModel>();
            foreach (var item in cart)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null) continue;

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

        /// <summary>
        /// Lấy danh sách kiểm tra khi chuẩn bị đặt hàng
        /// </summary>
        public List<CartItemViewModel> GetCheckoutItems()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                throw new InvalidOperationException("Giỏ hàng đang trống.");
            }

            var productIds = cart.Select(item => item.ProductId).ToList();
            var products = _context.Products.Where(product => productIds.Contains(product.Id)).ToList();

            var result = new List<CartItemViewModel>();
            foreach (var item in cart)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Sản phẩm mã {item.ProductId} không còn tồn tại.");
                }
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException($"Số lượng của {product.Name} không hợp lệ.");
                }
                if (item.Quantity > product.StockQuantity)
                {
                    throw new InvalidOperationException($"{product.Name} chỉ còn {product.StockQuantity} chiếc.");
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

        /// <summary>
        /// Xóa sạch giỏ hàng (trong cả Database và Session)
        /// </summary>
        public void ClearCart()
        {
            int? userId = CurrentUserId;
            if (userId.HasValue)
            {
                var dbItems = _context.CartItems.Where(c => c.UserId == userId.Value).ToList();
                if (dbItems.Any())
                {
                    _context.CartItems.RemoveRange(dbItems);
                    _context.SaveChanges();
                }
            }

            Session?.Remove(CartKey);
        }
    }
}