namespace ShopApp.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; } // لربط السلة بالمستخدم
        public List<CartItem> Items { get; set; } = new();
    }
}
