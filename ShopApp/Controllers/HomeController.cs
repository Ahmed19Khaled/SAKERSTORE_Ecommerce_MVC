using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Data;
using ShopApp.ViewModel;
using System.Diagnostics;


namespace ShopApp.Controllers
{
 
    public class HomeController : Controller
    {
        private readonly AppDBcontext context;

        public HomeController(AppDBcontext logger)
        {
            context = logger;
        }

        public IActionResult Index()
        {

            var Hview = new HViewModel {
              Categories=context.Categories.ToList(),
              SubCategories=context.CategoriesSup.ToList(),
              Products = context.Products.Include(x=>x.Images).Include(x=>x.Stocks).ToList(),
            };

            return View(Hview);
        }

        //public PartialViewResult Featuredproducts()
        //{
        //    var prod=context.Products
        //        .OrderByDescending(p=> p.Id)
        //        .Take(20).ToList();
        //    return PartialView(prod);
        //}
        public IActionResult Brands()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Deals()
        {
            return View();
        }

        public IActionResult NewArrivals()
        {
            var products = context.Products
                         .Include(p => p.SubCategory)
                         .Include(p => p.Images)
                         .Include(c => c.Stocks)
                         .OrderByDescending(p => p.Id)
                         .Take(15)
                         .ToList();

            var model = new AllProductsDitals
            {
                products = products,
            };

            //if (Categoryid.HasValue)
            //    products.Where(x => x.Id == Categoryid.Value);
            return View(model);
        }

        [HttpPost]
        public IActionResult SendMessage(string name, string email, string phone, string subject, string message)
        {
            // Handle contact form submission
            // You can save to database or send email here
            TempData["Success"] = "Your message has been sent successfully!";
            return RedirectToAction("Contact");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
