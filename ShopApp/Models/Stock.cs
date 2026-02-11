using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopApp.Models
{
    public class Stock
    {
        [Key]
        public int StockId { get; set; }

        public string? Size { get; set; }

        public string? Color { get; set; }

        [Required(ErrorMessage = "QTY is Required")]
        [Range(0, int.MaxValue, ErrorMessage = "Must be price over to 0")]
        public int QuantityAvaiable { get; set; } = 0;
        [Column(TypeName = "date")]

        public DateTime AddedDate { get; set; } = DateTime.Today;
        [Column(TypeName = "date")]

        public DateTime? LastUpdated { get; set; } =DateTime.Today;  

        // Relationships
        public int ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; }
    }
}
