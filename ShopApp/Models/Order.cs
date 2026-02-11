using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? UserId { get; set; } // صاحب الطلب
        [Column(TypeName = "date")]
        public DateTime OrderDate { get; set; } = DateTime.Today;
        [Required,MaxLength(150)]
        public string FullName { get; set; }

        [Required]
        [Phone]
        [MaxLength(11),MinLength(11)]
        public string PhoneNumber { get; set; }
        [MaxLength(200)]
        [Required]
        public string Address { get; set; }
        [MaxLength(100)]
        [Required]
        public string? Country { get; set; }
        [Required]
        [MaxLength(100)]
        public string? City { get; set; }
        
        public decimal TotalAmount { get; set; }
        public bool? ISPaid { get; set; } = false; // حالة الدفع، افتراضيًا غير مدفوع

        public DateTime? PaymentDate { get; set; } // تاريخ الدفع، إذا تم الدفع
         public string? PaymobeOrderId { get; set; } // معرف الطلب في Paymob، إذا تم الدفع عبر Paymob
        public string? Status { get; set; } = OrderStatus.Pending.ToString(); // يمكن أن تكون: Pending, Processing, Shipped, Delivered, Canceled

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
    public enum OrderStatus
    {
        Pending,
        Paid,
        Shipped,
        Delivered,
        Cancelled
    }


}
