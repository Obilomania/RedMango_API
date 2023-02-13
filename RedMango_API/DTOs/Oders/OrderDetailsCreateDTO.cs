using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RedMango_API.DTOs.Oders
{
    public class OrderDetailsCreateDTO
    {
        public int MenuItemId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public string ItemName { get; set; }
        [Required]
        public double Price { get; set; }
    }
}
