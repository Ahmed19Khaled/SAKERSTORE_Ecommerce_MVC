using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Data;
using ShopApp.Models;
using ShopApp.ViewModel;

namespace ShopApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDBcontext context;
        private readonly IWebHostEnvironment _environment;

        public CategoryController(AppDBcontext context, IWebHostEnvironment environment)
        {
            this.context = context;
            _environment = environment;
        }

        private string UploadImage(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "imageCategory");
            Directory.CreateDirectory(uploadsFolder);

            string originalFileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, originalFileName);
            
            // if exist 2 file
            int count = 1;
            while (System.IO.File.Exists(filePath))
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
                string extension = Path.GetExtension(originalFileName);
                string newFileName = $"{fileNameWithoutExt}_{count}{extension}";
                filePath = Path.Combine(uploadsFolder, newFileName);
                count++;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return Path.GetFileName(filePath);
        }
        public bool DeleteImage(string imageName)
        {
            // التأكد أن الصورة ليست صورة افتراضية
            if (!string.IsNullOrEmpty(imageName) && imageName != "DefultImage.jpg")
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imageCategory", imageName);

                // التأكد من وجود الصورة
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        // حذف الصورة
                        System.IO.File.Delete(imagePath);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // يمكنك التعامل مع الخطأ هنا
                        Console.WriteLine("Error deleting image: " + ex.Message);
                        return false;
                    }
                }
            }
            return false;
        }

        public IActionResult PruductCategory(int? Categoryid, string name, string description, int? SubCatigoryid, decimal? minPrice, decimal? maxPrice, int? offer)
        {
            var products = context.Products
                .Include(p => p.SubCategory)
                .Include(p=>p.Images)
                .Include(c=>c.Stocks)
          .Where(p =>
            (!SubCatigoryid.HasValue && (!Categoryid.HasValue || p.SubCategory.CategoryId == Categoryid))
            || (SubCatigoryid.HasValue && p.SubCategoryId == SubCatigoryid)
            )
                .Where(p => string.IsNullOrEmpty(name) || p.Name.Contains(name))
                .Where(p => string.IsNullOrEmpty(description) || p.Description.Contains(description))
                .Where(p => !SubCatigoryid.HasValue || p.SubCategoryId == SubCatigoryid)
                .Where(p => !minPrice.HasValue || p.Price >= minPrice)
                .Where(p => !maxPrice.HasValue || p.Price <= maxPrice)
                .Where(p => !offer.HasValue || (p.Offer.HasValue && p.Offer >= offer))
                .ToList();
            var model = new AllProductsDitals
            {
                Category = context.Categories.Where(x => x.Id == Categoryid).FirstOrDefault(),
                products = products,
                SubCategory= context.CategoriesSup.Include(x=>x.Category).Where(x=>x.Id == SubCatigoryid).FirstOrDefault(),
            };

            //if (Categoryid.HasValue)
            //    products.Where(x => x.Id == Categoryid.Value);
            return View(model);
        }
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult listCategory()
        {
            var cat = context.Categories.ToList();
                  return View(cat);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CreateCategory()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]

        public IActionResult CreateCategory(Category category)
        {
            if (category.formFile != null) { 
                category.ImageDepartment=UploadImage(category.formFile);
            }
            if (ModelState.IsValid) {
               context.Categories.Add(category); 
                context.SaveChanges();
                return RedirectToAction(nameof(listCategory));
            }
            return View();
        }
        [Authorize(Roles = "Admin,Manager")]

        public IActionResult EditCategory(int id)
        {  
            var existingCategory = context.Categories.FirstOrDefault(c => c.Id == id);
            return View(nameof(CreateCategory),existingCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]

        public IActionResult EditCategory(Category category)
        {
           var findcat=context.Categories.Find(category.Id);
            if (findcat == null) return NotFound();
            // if image is upload
            if (ModelState.IsValid)
            {
                findcat.NameDepartment=category.NameDepartment;
                if (category.formFile != null)
                {
                    DeleteImage(findcat.ImageDepartment);
                    findcat.ImageDepartment = UploadImage(category.formFile);

                }
                context.SaveChanges();
                return RedirectToAction(nameof(listCategory));
            }


            // في حالة وجود خطأ في النموذج، عرض النموذج مع البيانات السابقة
            return View(nameof(CreateCategory),findcat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]

        public IActionResult DeleteCategory(int id) {
           
            var cat=context.Categories.Find(id);
            if (cat == null) return NotFound();
            DeleteImage(cat.ImageDepartment);
               context.Categories.Remove(cat);
               context.SaveChanges();
            return RedirectToAction(nameof(listCategory));
        
        }




    }
}
 /*    public void UploadImage(Category category, string oldImage)
            {
                var file = HttpContext.Request.Form.Files;

                if (file.Count > 0)
                {
                    // حفظ الصورة الجديدة
                    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(file[0].FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imageCategory", imageName);

                    using (var filestream = new FileStream(filePath, FileMode.Create))
                    {
                        file[0].CopyTo(filestream);
                    }

                    category.ImageDepartment = imageName;
                }
                else
                {
                    // الاحتفاظ بالصورة القديمة
                    category.ImageDepartment = oldImage ?? "DefultImage.jpg";
                }
            }        */