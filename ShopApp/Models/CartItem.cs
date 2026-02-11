using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopApp.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        [ForeignKey("Stock")]
        public int? StockID { get; set; }
        [ValidateNever]
        public Product Product { get; set; }
        [ValidateNever]
        public Stock? Stock { get; set; }
        public int Quantity { get; set; }
    }
}