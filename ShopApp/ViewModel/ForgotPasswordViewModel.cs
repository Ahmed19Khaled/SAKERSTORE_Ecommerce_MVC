using System.ComponentModel.DataAnnotations;

namespace ShopApp.ViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "يرجى إدخال البريد الإلكتروني")]
        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        public string Email { get; set; }
    }
}