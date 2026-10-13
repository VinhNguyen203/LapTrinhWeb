using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LaptopShop.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Product>? Products { get; set; }
    }
}