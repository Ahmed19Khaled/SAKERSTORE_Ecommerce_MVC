using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Data;
using ShopApp.Models;
using ShopApp.Services;
using ShopApp.ViewModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using X.Paymob.CashIn;
using X.Paymob.CashIn.Models.Callback;


namespace ShopApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly AppDBcontext _context;
        private readonly UserManager<User> _userManager;
        private readonly PaymobService _paymob;
        private readonly IConfiguration _config;
        private readonly IPaymobCashInBroker _broker;

        public OrderController(AppDBcontext context,IPaymobCashInBroker inBroker, UserManager<User> userManager, PaymobService paymob, IConfiguration _config)
        {
            _context = context;
            _userManager = userManager;
            _paymob = paymob;
            this._config = _config;
            _broker = inBroker;

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
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
                return View(model);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            decimal total = 0;

            foreach (var item in cart.Items)
            {
                var price = CalculatePrice(item.Product);
                total += price * item.Quantity;
            }

            // ================= ONLINE PAYMENT =================
            if (model.PaymentMethod == PaymentMethod.Online)
            {
                var order = BuildOrder(model, user.Id, cart, total);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var token = await _paymob.GetAuthTokenAsync();

                var paymobOrderId = await _paymob.CreateOrderAsync(
                    token,
                    (int)(total * 100),
                    order.Id.ToString()
                );

                var paymentToken = await _paymob.GetPaymentKeyAsync(
                    token,
                    paymobOrderId,
                    (int)(total * 100),
                    model.FullName,
                    user.Email,
                    model.PhoneNumber
                );

                if (string.IsNullOrEmpty(paymentToken))
                {
                    ModelState.AddModelError("", "فشل في الحصول على مفتاح الدفع");
                    return View(model);
                }

                order.PaymobeOrderId = paymobOrderId.ToString();
                await _context.SaveChangesAsync();

                return RedirectToAction("RedirectToPaymob", new { token = paymentToken });
            }

            // ================= CASH ON DELIVERY =================
            else if (model.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                using var dbTransaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var order = BuildOrder(model, user.Id, cart, total);

                    foreach (var item in order.Items)
                    {
                        var stock = await _context.Stocks
                            .FirstOrDefaultAsync(s => s.StockId == item.StockID);

                        if (stock == null || stock.QuantityAvaiable < item.Quantity)
                            return BadRequest("المنتج غير متوفر حالياً");

                        stock.QuantityAvaiable -= item.Quantity;
                    }

                    _context.Orders.Add(order);
                    _context.CartItems.RemoveRange(cart.Items);

                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    return BadRequest("حدث خطأ أثناء تنفيذ الطلب");
                }
            }

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

        // callback endpoint for Paymob to notify about payment status
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PaymobWebhook(
      [FromQuery] string hmac,
      [FromBody] CashInCallback callbackData)
        {
            if (callbackData?.Type != CashInCallbackTypes.Transaction)
                return BadRequest();

            if (callbackData.Obj is not JsonElement jsonElement)
                return BadRequest();

            var rawJson = jsonElement.GetRawText();

            var transaction = JsonSerializer.Deserialize<CashInCallbackTransaction>(
                rawJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (transaction == null)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(hmac))
                return Unauthorized();

            if (!_broker.Validate(transaction, hmac))
                return Unauthorized();

            var merchantId = transaction.Order?.MerchantOrderId?.ToString();

            if (!int.TryParse(merchantId, out var localOrderId))
                return BadRequest();

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == localOrderId);

            if (order == null)
                return BadRequest();

            // Idempotency
            if (order.ISPaid == true)
                return Ok();

            // تحقق من المبلغ
            if (transaction.AmountCents != (int)(order.TotalAmount * 100))
            {
                order.Status = "AmountMismatch";
                await _context.SaveChangesAsync();
                return Ok();
            }

            if (transaction.Success && !transaction.Pending)
            {
                using var dbTransaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    order.ISPaid = true;
                    order.Status = "Paid";
                    order.PaymentDate = DateTime.UtcNow;

                    foreach (var item in order.Items)
                    {
                        var stock = await _context.Stocks
                            .FirstOrDefaultAsync(s => s.StockId == item.StockID);

                        if (stock == null || stock.QuantityAvaiable < item.Quantity)
                            throw new Exception("Stock inconsistency");

                        stock.QuantityAvaiable -= item.Quantity;
                    }

                    var cart = await _context.Carts
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.UserId == order.UserId);

                    if (cart != null)
                        _context.CartItems.RemoveRange(cart.Items);

                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    return Ok(); // مهم جداً
                }
            }
            else
            {
                order.Status = "Failed";
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        private string CalculateHmacHex(string rawJson, string secret)
        {
            byte[] key;
            if (!string.IsNullOrEmpty(secret) &&
                System.Text.RegularExpressions.Regex.IsMatch(secret, @"\A[0-9a-fA-F]+\z") &&
                secret.Length % 2 == 0)
            {
                // .NET 6+ Convert.FromHexString
                key = Convert.FromHexString(secret);
            }
            else
            {
                key = Encoding.UTF8.GetBytes(secret);
            }

            using var hmac = new HMACSHA512(key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawJson));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }


        [AllowAnonymous]
        public IActionResult PaymobResult()
        {
            var successParam = Request.Query["success"].ToString();
            var orderId = Request.Query["order"].ToString();
            var message = Request.Query["data.message"].ToString();

            bool isSuccess = successParam == "true";


            if (isSuccess)
            {
                return RedirectToAction("OrderConfirmationSuccses");
            }

            return RedirectToAction("Failure");
        }


        [AllowAnonymous]
        public IActionResult OrderConfirmationSuccses()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult Failure()
        {
            return View();
        }

        private decimal CalculatePrice(Product product)
        {
            var price = product.Price;

            if (product.Offer.HasValue)
                price -= price * product.Offer.Value / 100m;

            return price;
        }
        private Order BuildOrder(
            OrderCheckoutViewModel model,
            string userId,
            Cart cart,
            decimal total)
        {
            var order = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = total,
                ISPaid = false,
                Items = new List<OrderItem>()
            };

            foreach (var item in cart.Items)
            {
                var price = CalculatePrice(item.Product);

                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = price,
                    StockID = item.StockID
                });
            }

            return order;
        }

    }

}
