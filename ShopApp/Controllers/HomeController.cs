using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Data;

using ShopApp.ViewModel;


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






        public IActionResult Privacy()
        {
            return View();
        }





     /*
        public PartialViewResult GetDepartment()
        {
            var categ=context.Categories.ToList();
            return PartialView( categ);
        }

        public PartialViewResult GetCothesDep()
        {
            var subCategories = context.CategoriesSup.ToList();

            return PartialView(subCategories);
        }
      */
    }
}
