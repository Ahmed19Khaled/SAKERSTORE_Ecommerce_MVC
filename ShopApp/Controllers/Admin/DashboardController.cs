using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Data;
using ShopApp.Models;
using ShopApp.ViewModel;

namespace ShopApp.Controllers.Admin
{
    [Authorize(Roles = "Admin,Manager")]

    public class AdminController : Controller
    {
        private readonly AppDBcontext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(AppDBcontext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var usersCount = _userManager.Users.Count();
            var totalOrders = _context.Orders.Count();
            var totalRevenue = _context.Orders.Where(o => o.ISPaid == true).Sum(o => o.TotalAmount);
            var ordertoday = _context.Orders.Count(o => o.OrderDate.Date == DateTime.Today);
            var productsCount = _context.Products.Count();

            var latestOrders = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(x=>x.Images)
                .OrderByDescending(o => o.OrderDate)
                .Take(12)
                .ToList();

            var topProducts = _context.OrderItems
                .Include(x => x.Product)
                    .ThenInclude(p => p.Images)
                .GroupBy(x => x.ProductId)
                .Select(g => new TopProductViewModel
                {
                    ProductName = g.First().Product.Name,
                    TotalSold = g.Sum(x => x.Quantity),
                    Category = g.First().Product.SubCategory.Name,
                    Price = g.First().Product.Price,
                    ImageUrl = g.First().Product.Images.FirstOrDefault().ImageURL,
                    SaleDate=g.First().Product.CreatAT,     // 👈 صورة واحدة فقط
                    isavilabel = g.First().Product.Stocks.Any(s => s.QuantityAvaiable > 0), // تحقق من توفر المنتج
                    TotalQTY = _context.Stocks
                        .Where(s => s.ProductId == g.First().ProductId)
                        .Sum(s => s.QuantityAvaiable),// إجمالي الكمية المتاحة  
                    
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToList();

            var currentYear = DateTime.Now.Year;

            var monthlyIncome = _context.Orders
    .Where(o => o.ISPaid == true && o.OrderDate.Year == currentYear)
    .GroupBy(o => o.OrderDate.Month)
    .Select(g => new
    {
        MonthNumber = g.Key,
        Amount = g.Sum(x => x.TotalAmount)
    })
    .ToList();

            // تحويل الرقم إلى اسم الشهر
            var monthlyIncomeData = monthlyIncome
                .OrderBy(x => x.MonthNumber)
                .Select(x => new MonthlyIncomeData
                {
                    Month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(x.MonthNumber),
                    Amount = x.Amount
                })
                .ToList();



            var lowStockProducts = _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.QuantityAvaiable < 10)
                .ToList();

            var viewModel = new AdminDashboardViewModel
            {
                UsersCount = usersCount,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                LatestOrders = latestOrders,
                TopProducts = topProducts,
                LowStockProducts = lowStockProducts,
                ordertoday = ordertoday,
                ProductsCount = productsCount,
                Followers = _userManager.Users.Count(),
                MonthlyIncome = monthlyIncomeData, // ← هنا
                TotalVisitors = _userManager.Users.Count(),
                imageuser = user.ProfilePictureUrl,
                User = user
            };


            return View(viewModel);
        }

    }

}
