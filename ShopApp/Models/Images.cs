using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Build.Framework;

namespace ShopApp.Models
{
    public class Images
    {
        [Key]
        [MaxLength(450)]
        public string ImageId { get; set; }= Guid.NewGuid().ToString();
        public string? ImageURL { get; set; }

        // Relationship
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

    }
}
