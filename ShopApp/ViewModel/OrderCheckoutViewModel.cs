using System.ComponentModel.DataAnnotations;

namespace ShopApp.ViewModel
{
    public class OrderCheckoutViewModel
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; }

        [Required, Phone, MaxLength(11), MinLength(11)]
        public string PhoneNumber { get; set; }

        [Required, MaxLength(200)]
        public string Address { get; set; }

        [Required, MaxLength(100)]
        public string Country { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public decimal TotalAmount { get; set; } // للعرض فقط مثلاً

        // يمكنك إضافة CartItems أو خيارات إضافية هنا إذا أردت
    } 
    
    public enum PaymentMethod
        {
            Online,
            CashOnDelivery
        }

}        