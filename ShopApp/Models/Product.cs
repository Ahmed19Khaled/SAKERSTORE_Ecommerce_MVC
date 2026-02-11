using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [StringLength(600)]
        
        public string Description { get; set; }
        [Required]
        [Range(0.01, 1000000, ErrorMessage = "Rang  of Price between ")]                                   
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
        [Column(TypeName = "date")]
        public DateTime CreatAT { get; set; } = DateTime.Today;
        [Range(0, 100)]
        [Display(Name = "Offer(%)")]
        public int? Offer {  get; set; } 
        [ForeignKey("SubCategory")]
        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }
        public required List<Images> Images { get; set; }
        public List<Stock> Stocks { get; set; }
    }
}