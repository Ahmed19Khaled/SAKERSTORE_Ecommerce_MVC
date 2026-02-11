    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity;
    using ShopApp.Models;
    using ShopApp.Data;
using ShopApp.ViewModel;
using ShopApp.Services;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;


namespace ShopApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly AppDBcontext _context;
        private readonly UserManager<User> _userManager;
        private readonly PaymobService _paymob;
        private readonly IConfiguration _config;

        public OrderController(AppDBcontext context, UserManager<User> userManager, PaymobService paymob, IConfiguration _config)
        {
            _context = context;
            _userManager = userManager;
            _paymob = paymob;
            this._config = _config;
        }  
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var total = cart.Items.Sum(i =>
            {
                var price = i.Product.Price;
                if (i.Product.Offer.HasValue)
                    price -= price * i.Product.Offer.Value / 100m;
                return price * i.Quantity;
            });

            var model = new OrderCheckoutViewModel
            {
                TotalAmount = total
                
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(OrderCheckoutViewModel model)
        {
            var user =await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
                return View(model);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var total = cart.Items.Sum(i =>
            {
                var price = i.Product.Price;
                if (i.Product.Offer.HasValue)
                    price -= price * i.Product.Offer.Value / 100m;
                return price * i.Quantity;
            });

            if (model.PaymentMethod == PaymentMethod.Online)
            {
                var token = await _paymob.GetAuthTokenAsync();
                var orderId = await _paymob.CreateOrderAsync(token, (int)(total * 100)); // Paymob uses cents
                var paymentToken = await _paymob.GetPaymentKeyAsync(token, orderId, (int)(total * 100), model.FullName, user.Email, model.PhoneNumber);
                if (string.IsNullOrEmpty(paymentToken))
                {
                    ModelState.AddModelError("", "فشل في الحصول على مفتاح الدفع من Paymob");
                    return View(model);
                }
                var order = new Order
                {
                    UserId = user.Id,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    Country = model.Country,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending.ToString(), // الحالة اللوجستية
                    TotalAmount = total,
                    PaymobeOrderId = orderId.ToString(),    
                    ISPaid = false,
                };


                foreach (var item in cart.Items)
                {
                    var price = item.Product.Price;
                    if (item.Product.Offer.HasValue)
                        price -= price * item.Product.Offer.Value / 100m;

                    order.Items.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = price,
                        StockID = item.StockID
                    });
                    var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.StockId == item.StockID);
                    if (stock != null)
                    {
                        stock.QuantityAvaiable -= item.Quantity;
                        if (stock.QuantityAvaiable < 0)
                            stock.QuantityAvaiable = 0; // أو يمكنك رفض الطلب إذا لم يكن هناك كمية كافية
                    }

                }

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();

                return RedirectToAction("RedirectToPaymob", new { token = paymentToken });
            }
            else if (model.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                var order = new Order
                {
                    UserId = user.Id,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    Country = model.Country,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending.ToString(), // قيد التجهيز
                    TotalAmount = total,
                    ISPaid = false                            // لأنه عند الاستلام
                };


                foreach (var item in cart.Items)
                {
                    var price = item.Product.Price;
                    if (item.Product.Offer.HasValue)
                        price -= price * item.Product.Offer.Value / 100m;

                    order.Items.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = price
                    });
                }

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();

                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }

            // ✅ fallback return to avoid missing return path
            return BadRequest("طريقة الدفع غير مدعومة");
        }
        public IActionResult RedirectToPaymob(string token)
        {
            ViewBag.Ifram = _config["PayMob:IframeId"];
            ViewBag.Token = token;
            return View();
        }

        public async Task<IActionResult> OrderConfirmation(int orderId)
         {
            var order = await _context.Orders
                .Include(o => o.Items)
                      .ThenInclude(oi => oi.Product)
                      .ThenInclude(c => c.Images)
                .Include(c => c.Items)
                      .ThenInclude(i => i.Stock)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            return View(order);
         }
     
        public async Task<IActionResult> EditStatuse(string status, int id)
        {
            var order = _context.Orders.FirstOrDefault(x => x.Id == id);
            if (order == null)
                return NotFound();
               order.Status = status;
              _context.SaveChanges();
            return RedirectToAction("Index","Home");
        }

        public async Task<IActionResult> OrderConfirmationSuccses()
        {
            // من الأفضل استقبال بيانات من Paymob للتحقق، لكن يمكن فقط عرض رسالة هنا
            return View(); // أنشئ View باسم PaymentResult.cshtml
        }


        [HttpPost]
        [Route("webhook/paymob")]
        public async Task<IActionResult> PaymobWebhook([FromBody] JsonElement payload)
        {
            try
            {
                var hmacSecret =_config["PayMob:Hmac"]; // أضف هذا في appsettings
                var receivedSignature = Request.Headers["hmac"].ToString();

                // التحقق من التوقيع
                var calculatedSignature = CalculateHMAC(payload.GetRawText(), hmacSecret);
                if (receivedSignature != calculatedSignature)
                    return Unauthorized("Invalid HMAC signature");

                var type = payload.GetProperty("type").GetString();
                if (type != "TRANSACTION")
                    return Ok(); // نهتم فقط بمعاملات الدفع

                var data = payload.GetProperty("obj");
                var success = data.GetProperty("success").GetBoolean();
                var isPaid = data.GetProperty("is_paid").GetBoolean();

                if (success && isPaid)
                {
                    var orderId = data.GetProperty("order").GetInt32();

                    // الحصول على الطلب المرتبط
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.PaymobeOrderId == orderId.ToString());
                    if (order != null && order.Status != "Paid")
                    {
                        order.ISPaid = true;
                        order.PaymentDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // سجل الخطأ لسهولة تتبعه
                Console.WriteLine("Webhook Error: " + ex.Message);
                return StatusCode(500);
            }
        }

        private string CalculateHMAC(string rawJson, string secret)
        {
            var key = Encoding.UTF8.GetBytes(secret);
            var message = Encoding.UTF8.GetBytes(rawJson);

            using var hmac = new HMACSHA512(key);
            var hash = hmac.ComputeHash(message);

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }




    }

}
