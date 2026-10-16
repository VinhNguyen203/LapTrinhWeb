using System.ComponentModel.DataAnnotations;
using System.Data;
using LaptopShop.Models;
using Microsoft.EntityFrameworkCore;

namespace LaptopShop.Services
{
    public class CheckoutService
    {
        private readonly AppDbContext _context;
        private readonly CartService _cartService;

        public CheckoutService(
            AppDbContext context,
            CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public int PlaceOrder(CheckoutViewModel input, string? userId = null)
        {
            Validator.ValidateObject(
                input,
                new ValidationContext(input),
                validateAllProperties: true);

            var strategy = _context.Database.CreateExecutionStrategy();

            return strategy.Execute(() =>
            {
                using var transaction = _context.Database
                    .BeginTransaction(IsolationLevel.Serializable);

                var items = _cartService.GetCheckoutItems();

                var order = new Order
                {
                    UserId = userId,
                    CustomerName = input.CustomerName.Trim(),
                    PhoneNumber = input.PhoneNumber.Trim(),
                    ShippingAddress = input.ShippingAddress.Trim(),
                    OrderDate = DateTime.Now,
                    TotalAmount = items.Sum(item => item.TotalPrice),
                    Status = "Pending"
                };

                foreach (var item in items.OrderBy(item => item.ProductId))
                {
                    int affectedRows = _context.Products
                        .Where(product =>
                            product.Id == item.ProductId &&
                            product.StockQuantity >= item.Quantity)
                        .ExecuteUpdate(setters => setters.SetProperty(
                            product => product.StockQuantity,
                            product => product.StockQuantity - item.Quantity));

                    if (affectedRows != 1)
                    {
                        throw new InvalidOperationException($"{item.ProductName} không còn đủ tồn kho.");
                    }

                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }

                _context.Orders.Add(order);
                _context.SaveChanges();

                transaction.Commit();

                return order.Id;
            });
        }
    }
}