using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShopApp.Data;
using ShopApp.Models;
using ShopApp.ViewModel;

namespace ShopApp.Controllers.Admin
{
    [Authorize(Roles = "Admin,Manager")]

    public class OrderManegController : Controller
    {
        private readonly AppDBcontext _context;
        private readonly UserManager<User> _userManager ;

        public OrderManegController(AppDBcontext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult GetAllOrder(string status)
        {
            var liststatus = new List<string>
                {
                    "Pending", "Processing", "Shipped", "Delivered", "FailedDelivery", "Cancelled"
                };

            ViewBag.SelectedStatus = status;
            ViewBag.OrderStatus = new SelectList(liststatus, status);


            var orders = _context.Orders
                .Include(x => x.Items)
                .ThenInclude(c => c.Product)
                .AsQueryable(); // نحتاج لإبقاء الاستعلام قابل للتصفية

            if (!string.IsNullOrEmpty(status))
                orders = orders.Where(x => x.Status == status);

            return View(orders.ToList()); // نفذ الاستعلام بعد التصفية
        }

        [HttpGet]
        public async Task<IActionResult> Details(int orderId)
        {
            var order = await _context.Orders
                .Include(x => x.Items)
                .ThenInclude(c => c.Product)
                .ThenInclude(c=>c.Images)
                .Include(c => c.Items)
                .ThenInclude(i => i.Stock)
                .FirstOrDefaultAsync(x => x.Id == orderId); // استخدم Async

            if (order == null)
                return NotFound();

            var userid = order.UserId;
            var user = await _userManager.FindByIdAsync(userid);

            if (user == null)
                return NotFound();

            ViewBag.email = await _userManager.GetEmailAsync(user);
            return View(order);
        }


        [HttpPost] 
        public async Task<IActionResult> EditStatuse(string status,int id)
        {
            var order=_context.Orders.FirstOrDefault(x => x.Id == id);
            order.Status=status;
            _context.SaveChanges();

            return RedirectToAction(nameof(GetAllOrder));
        }
    
        public IActionResult Delete(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                _context.Orders.RemoveRange(order);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(GetAllOrder));
        }
    }
}



//public async Task<IActionResult> Checkout(OrderCheckoutViewModel model)
//{
//    var userId = _userManager.GetUserId(User);

//    if (!ModelState.IsValid)
//        return View(model);

//    var cart = await _context.Carts
//        .Include(c => c.Items)
//        .ThenInclude(i => i.Product)
//        .FirstOrDefaultAsync(c => c.UserId == userId);

//    if (cart == null || !cart.Items.Any())
//        return RedirectToAction("Index", "Cart");

//    var total = cart.Items.Sum(i =>
//    {
//        var price = i.Product.Price;
//        if (i.Product.Offer.HasValue)
//            price -= price * i.Product.Offer.Value / 100m;
//        return price * i.Quantity;
//    });

//    // 1. طلب التوكن
//    var token = await _paymob.GetAuthTokenAsync();

//    // 2. إنشاء طلب PayMob
//    var paymobOrderId = await _paymob.CreateOrderAsync(token, total * 100);

//    // 3. طلب payment token
//    var paymentToken = await _paymob.GetPaymentKeyAsync(token, paymobOrderId, total * 100, model.FullName, "email@site.com", model.PhoneNumber);

//    // حفظ بيانات الطلب محليًا كما تفعل بالفعل
//var order = new Order { ... }; // كما فعلت في الكود الحالي
     //    _context.Orders.Add(order);
    //    _context.CartItems.RemoveRange(cart.Items);
   //    await _context.SaveChangesAsync();

//    // 4. توجيه المستخدم إلى صفحة الدفع
//    return RedirectToAction("Pay", new { token = paymentToken });
//}






/*     [HttpPost]
     [ValidateAntiForgeryToken]
     public async Task<IActionResult> Create(int id, Order order, int[] ProductIds, int[] Quantities)
     {

         if (id != order.Id)
             return NotFound();

         if (ModelState.IsValid)
         {
             try
             {
                 // تحديث بيانات الطلب الأساسية
                 _context.Update(order);

                 // إزالة العناصر القديمة
                 var oldItems = _context.OrderItems.Where(i => i.OrderId == id);
                 _context.OrderItems.RemoveRange(oldItems);

                 // إضافة عناصر جديدة
                 for (int i = 0; i < ProductIds.Length; i++)
                 {
                     var product = await _context.Products.FindAsync(ProductIds[i]);
                     if (product != null)
                     {
                         var item = new OrderItem
                         {
                             OrderId = order.Id,
                             ProductId = product.Id,
                             Quantity = Quantities[i],
                             UnitPrice = product.Price
                         };
                         _context.OrderItems.Add(item);
                     }
                 }

                 await _context.SaveChangesAsync();
             }
             catch (DbUpdateConcurrencyException)
             {
                 if (!_context.Orders.Any(e => e.Id == order.Id))
                     return NotFound();
                 else
                     throw;
             }

             return RedirectToAction(nameof(GetAllOrder));
         }

         ViewBag.Products = new SelectList(_context.Products, "Id", "Name");
         return View(order);
     }
       */