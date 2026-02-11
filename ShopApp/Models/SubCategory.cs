using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopApp.Models
{
    public class SubCategory
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [MaxLength(100)]
        public string? ImageDepartment { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; }
        [ValidateNever]
        public ICollection<Product> Products { get; set; }
        [NotMapped]
        public IFormFile? formFile { get; set; }

    }
}