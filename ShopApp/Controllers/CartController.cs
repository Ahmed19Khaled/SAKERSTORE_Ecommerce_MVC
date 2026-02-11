using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using ShopApp.Data;
using ShopApp.Models;
using ShopApp.Models.Services;
using System.Threading.Tasks;
namespace ShopApp.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;
        private  readonly IHttpContextAccessor _httpContextAccessor;


        private readonly AppDBcontext _context;
        public CartController(ICartService cartService, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager, AppDBcontext context)
        {
            _userManager = userManager;
            _cartService = cartService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;

        }

        public IActionResult AllCart()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Login), "Account");
            }
            var items = _cartService.GetCartItems();
            ViewBag.Total = _cartService.GetCartTotal();
            return View(items);
        }
        [HttpPost]
        public async Task<IActionResult> Add(int id,int stockid,int qty = 1)
        {
          //  var userid = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Login),"Account");
            }

            var product = _context.Products.Find(id);
            try
            {
               await _cartService.AddToCart(product, qty, stockid);
            }
            catch (InvalidOperationException ex)
            {
                TempData["CartError"] = ex.Message;
                return RedirectToAction("DetailsProduct", "Product", new { ProductID = id });
            }
            catch (Exception ex)
            {
                TempData["CartError"] = "حدث خطأ غير متوقع: " + ex.Message;
                return RedirectToAction("DetailsProduct", "Product", new { ProductID = id });
            }

            return RedirectToAction("AllCart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] QuantityUpdateModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, redirectUrl = Url.Action("Login", "Account") });
            }

            var product = _context.Products
                .FirstOrDefault(p => p.Id == model.ProductId);


            if (product == null)
                return Json(new { success = false, message = "المنتج غير موجود" });

            try
            {
                _cartService.AddToCart(product, model.Qty, model.StockId);

                decimal price = product.Price;
                if (product.Offer.HasValue)
                    price -= price * product.Offer.Value / 100;

                var itemTotal = price * model.Qty;
                var cartTotal = _cartService.GetCartTotal();

                return Json(new { success = true, itemTotal, cartTotal });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ أثناء تحديث السلة: " + ex.Message });
            }
        }

       
        public IActionResult Remove(int id)
        {
            _cartService.RemoveFromCart(id);
            return RedirectToAction("AllCart");
        }

        public IActionResult Clear()
        {
            _cartService.ClearCart();
            return RedirectToAction("AllCart");
        }
    }
}
 public class QuantityUpdateModel
        {
            public int ProductId { get; set; }
            public int StockId { get; set; }
            public int Qty { get; set; }
        }
