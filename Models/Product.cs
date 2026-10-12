using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
       
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public required string CPU { get; set; }
        public required string RAM { get; set; }
        public required string GPU { get; set; }
         
        public required int BrandId { get; set;  }
        public required int CategoryId { get; set; }
       
       


    }
}