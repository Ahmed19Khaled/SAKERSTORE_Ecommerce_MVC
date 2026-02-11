using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ShopApp.Models;
using static System.Net.Mime.MediaTypeNames;

namespace ShopApp.ViewModel
{
    public class AddProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is Required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Description is Required")]
        public string? Description { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Must be price over to 0")]
        public decimal Price { get; set; }

        public int? Offer { get; set; }
        [Required(ErrorMessage = "SubCategory is REQUIRED")]

        public int SubCategoryId { get; set; }

        [ValidateNever]
        [NotMapped]
        public List<IFormFile>? formFile { get; set; }

        public List<Images>? ExistingImages { get; set; }
        public List<Stock> Stocks { get; set; } = new List<Stock>();

    }
}