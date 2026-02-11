using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopApp.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }
        [ForeignKey("Stock")]
        public int? StockID { get; set; }
        [ValidateNever]
        public Stock Stock { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; } // السعر وقت الشراء (مع العرض إن وُجد)

        public decimal Total => Quantity * UnitPrice;
    }

}