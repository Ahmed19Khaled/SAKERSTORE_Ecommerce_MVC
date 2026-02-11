using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ShopApp.Models
{
    public class User : IdentityUser
    {
        // يمكنك إضافة خصائص إضافية هنا مثل الاسم الكامل مثلاً
        [Required,MaxLength(255)]
        public string FullName { get; set; }

        [MaxLength(255)]
        public string? ProfilePictureUrl { get; set; } = "/images/default-profile.png";

        [MaxLength(100)]
        public string? Country { get; set; }
        [MaxLength(100)]
        public string? City { get; internal set; }
        [MaxLength(50)]
        public string? Gender { get; set; } // Male / Female / Other
        [Column(TypeName = "date")]

        public DateTime? BirthDate { get; internal set; }
        [Column(TypeName = "date")]
        public DateTime CreatedAt { get; internal set; }

    }

}

