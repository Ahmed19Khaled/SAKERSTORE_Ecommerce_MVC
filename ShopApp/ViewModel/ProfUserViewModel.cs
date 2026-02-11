using ShopApp.Models;

namespace ShopApp.ViewModel
{
    public class ProfUserViewModel
    {

            public string Email { get; set; }

            public string FullName { get; set; }

            public string PhoneNumber { get; set; }

            public string? ProfilePictureUrl { get; set; }


            public string? City { get; set; }

            public string? Country { get; set; }
            public string? Role { get; set; }

            public DateTime? BirthDate { get; set; }

            public string? Gender { get; set; }


            public DateTime CreatedAt { get; set; }

            public List<Order>? Orders { get; set; } = new();
       



    }
}
