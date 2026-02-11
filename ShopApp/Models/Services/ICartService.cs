namespace ShopApp.Models.Services
{
    public interface ICartService
    {
        Task AddToCart(Product product, int quantity, int stockid);
        void RemoveFromCart(int productId);
        List<CartItem> GetCartItems();
        decimal GetCartTotal();
        void ClearCart();
    }
}