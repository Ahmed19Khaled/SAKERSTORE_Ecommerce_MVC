using Microsoft.Ajax.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Data;
using ShopApp.Models;
using ShopApp.ViewModel;

namespace ShopApp.ViewComponents
{
    public class CartSummaryViewComponent :ViewComponent
    {
        private readonly AppDBcontext context;
        private readonly UserManager<User> userManager  ;

        public CartSummaryViewComponent( AppDBcontext _context,UserManager<User> _userManager )
        {
            this.context = _context;
            this.userManager = _userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userid = userManager.GetUserId(HttpContext.User);
            if (userid == null)
            {
                return View(new CartLayoutViewModel());
            }

            var cart = context.Carts
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .ThenInclude(x=>x.Images)
                .FirstOrDefault(x => x.UserId == userid);

            if (cart == null)
            {
                return View(new CartLayoutViewModel());
            }

            var viewmodel = new CartLayoutViewModel()
            {
                Count = cart.Items.Sum(x => x.Quantity),
                cartItems = cart.Items.ToList(),
                Total = cart.Items.Sum(i =>
                {
                    var price = i.Product.Price;
                    if (i.Product.Offer.HasValue)
                        price -= price * i.Product.Offer.Value / 100m;
                    return price * i.Quantity;
                })
            };

            return View(viewmodel);
        }

    }
}
