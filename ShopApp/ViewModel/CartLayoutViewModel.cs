using ShopApp.Models;

namespace ShopApp.ViewModel
{
    public class CartLayoutViewModel
    {


        public int Count { get; set; }
        public    decimal  Total { get; set; }

        public List<CartItem > cartItems { get; set; }

    }
}
