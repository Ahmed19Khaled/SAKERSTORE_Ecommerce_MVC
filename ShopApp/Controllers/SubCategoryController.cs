using Microsoft.Ajax.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Data;
using ShopApp.Models;
using System.Numerics;

namespace ShopApp.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class SubCategoryController : Controller
    {
        private readonly AppDBcontext context;
        private readonly IWebHostEnvironment _environment;
        public SubCategoryController(AppDBcontext dBcontext,IWebHostEnvironment environment)
        {
            this.context = dBcontext;
            this._environment = environment;
      }
        private string UploadImage(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "imageSubCategory");
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
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imageSubCategory", imageName);

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

        public IActionResult listsubCategory()
        {
           var list=context.CategoriesSup.Include(x=>x.Category).OrderBy(x=>x.CategoryId).ToList();
            return View(list);
        }
        [HttpGet]
        public IActionResult CreateSubCategory()
        {
            ViewBag.Department = context.Categories.ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSubCategory(SubCategory subCategory)
        {
            ViewBag.Department = context.Categories.ToList();
            if (subCategory == null) return View();

            // تحقق من أن الـ ModelState صحيح
            if (ModelState.IsValid)
            {
                if (subCategory.formFile != null)
                {
                    
                    subCategory.ImageDepartment = UploadImage(subCategory.formFile); // رفع الصورة
                }

                context.CategoriesSup.Add(subCategory); // إضافة الـ SubCategory إلى الـ DB
                context.SaveChanges();
                return RedirectToAction(nameof(listsubCategory)); // إعادة التوجيه إلى القائمة
            }

            return View();
        }


        [HttpGet]
         public IActionResult EditSubCategory(int id)
        {
            ViewBag.Department = context.Categories.ToList();
            var subCategory = context.CategoriesSup.Find(id);
            if (subCategory == null) return View();
            return View(nameof(CreateSubCategory),subCategory);
        }    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSubCategory(SubCategory subCategory)
        {
            ViewBag.Department = context.Categories.ToList();
            var subfind = context.CategoriesSup.Find(subCategory.Id);
            if (subCategory == null) return View();
            if (ModelState.IsValid) {
              subfind.Name=subCategory.Name;
                //لو رفع صوره
                if(subCategory.formFile!= null)
                {
                    DeleteImage( subfind.ImageDepartment );
                    subfind.ImageDepartment = UploadImage(subCategory.formFile);
                }
                context.SaveChanges();
                return RedirectToAction(nameof(listsubCategory));
            }
        
            return View(nameof(CreateSubCategory),subfind);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int id)
        {

            var cat = context.CategoriesSup.Find(id);
            if (cat == null) return NotFound();
            DeleteImage(cat.ImageDepartment);
            context.CategoriesSup.Remove(cat);
            context.SaveChanges();
            return RedirectToAction(nameof(listsubCategory));

        }




    }
}