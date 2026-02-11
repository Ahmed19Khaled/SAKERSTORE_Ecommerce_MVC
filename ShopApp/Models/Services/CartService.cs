using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopApp.Data;
using ShopApp.Migrations;
using System.Threading.Tasks;

namespace ShopApp.Models.Services
{
    public class CartService : ICartService
    {
        private readonly AppDBcontext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(AppDBcontext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        private  string GetUserId()
        {
            return _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
        }

        private Cart GetOrCreateCart()
        {
            var userId = GetUserId();
        

            var cart = _context.Carts
                .Include(c => c.Items)
                     .ThenInclude(i => i.Product)
                     .ThenInclude(I => I.Images)
                 .Include(c => c.Items)
                     .ThenInclude(i => i.Stock)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return cart;
        }

        public async  Task AddToCart(Product product, int quantity , int stockid )
        {
            var cart = GetOrCreateCart();
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == product.Id && i.StockID == stockid);
            var stock = _context.Stocks.FirstOrDefault(s => s.StockId == stockid);
            if (stock == null)
                throw new InvalidOperationException("الرجاء اختيار اللون الخاص بالمخزن المحدد قبل الإضافة إلى السلة.");

            if (stock.QuantityAvaiable < quantity)
            {
                throw new InvalidOperationException("الكمية المطلوبة غير متوفرة في المخزن.");
            }

            if (existingItem != null && existingItem.Stock.Color == stock.Color)
            { 
                 existingItem.Quantity = quantity;
            } else
                cart.Items.Add(new CartItem { ProductId = product.Id, Quantity = quantity, StockID = stock.StockId });

            _context.SaveChanges();
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetOrCreateCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cart.Items.Remove(item);
                _context.SaveChanges();
            }
        }

        public List<CartItem> GetCartItems()
        {
            return GetOrCreateCart().Items.ToList();
        }

        public decimal GetCartTotal()
        {
            /*  cart.Items.Sum(i =>
              {
                  var price = i.Product.Price;
                  if (i.Product.Offer.HasValue)
                  {
                      price -= price * i.Product.Offer.Value / 100m;
                  }
                  return price * i.Quantity;
              })
              }; */
            return GetOrCreateCart().Items.Sum(i =>
            {
                var price = i.Product.Price;
                if (i.Product.Offer.HasValue)
                {
                    price -= price * i.Product.Offer.Value / 100m;
                }
                return price * i.Quantity;
            });
        }

        public void ClearCart()
        {
            var cart = GetOrCreateCart();
            cart.Items.Clear();
            _context.SaveChanges();
        }
    }
}
