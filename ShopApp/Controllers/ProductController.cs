
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Data;
using ShopApp.Models;
using ShopApp.ViewModel;

namespace ShopApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDBcontext context;
        private readonly IWebHostEnvironment environment;
        public ProductController(AppDBcontext dBcontext, IWebHostEnvironment _environment)
        {
            this.context = dBcontext;
            this.environment = _environment;
        }
        private string UploadImage(IFormFile file)
        {
            string uploadsFolder = Path.Combine(environment.WebRootPath, "ImgePro");
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
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ImgePro", imageName);

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

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult listProduct(string name, decimal? minPrice, decimal? maxPrice, int? subCategoryId)
        {
            IQueryable<Product> products = context.Products.Include(x => x.SubCategory);
            ViewBag.SubCategories = context.CategoriesSup.ToList();
            if (!string.IsNullOrEmpty(name))
            {
                products = products.Where(p => p.Name.Contains(name));
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            if (subCategoryId.HasValue)
            {
                products = products.Where(p => p.SubCategoryId == subCategoryId.Value);
            }
            var result = products.OrderBy(p => p.Name).Include(x => x.Images).ToList();
            // لملء القائمة المنسدلة
            return View(result);
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]

        public IActionResult CreateProduct()
        {
            ViewBag.Departments = context.CategoriesSup.OrderBy(x => x.Name).ToList();

            var model = new AddProductViewModel
            {
                Id=0,
                formFile = new List<IFormFile>()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> CreateProduct(AddProductViewModel product)
        {
            ViewBag.Departments = context.CategoriesSup.OrderBy(x => x.Name).ToList();

            if (!ModelState.IsValid)
                return View(product);
            var pro = new Product
            {
                Description = product.Description,
                Name = product.Name,
                Offer = product.Offer,
                Price = product.Price,
                SubCategoryId = product.SubCategoryId,
                Images = new List<Images>()
            };

            context.Products.Add(pro);
            await context.SaveChangesAsync();

            if (product.formFile != null)
            {
                foreach (var image in product.formFile)
                {
                    var img = new Images
                    {
                        ImageURL = UploadImage(image),
                        ProductId = pro.Id
                    };
                    context.Images.Add(img);
                }
                await context.SaveChangesAsync();
            }
            if (product.Stocks != null && product.Stocks.Any())
            {
                foreach (var stock in product.Stocks)
                {
                    // ✅ الشرط: لازم يكون في لون + كمية للون ده
                    if (!string.IsNullOrWhiteSpace(stock.Color) && stock.QuantityAvaiable > 0)
                    {
                        stock.ProductId = pro.Id;
                        stock.AddedDate = DateTime.Now;
                        context.Stocks.Add(stock);
                    }
                }

                await context.SaveChangesAsync();
            }


            return RedirectToAction(nameof(listProduct));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]

        public IActionResult EditProduct(int id)
        {
            ViewBag.Departments = context.CategoriesSup.OrderBy(x => x.Name).ToList();
            var product = context.Products
             .Include(p => p.Images)
             .Include(p => p.Stocks)
             .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            var vm = new AddProductViewModel
            {
                Id = id,
                Description = product.Description,
                Name = product.Name,
                Offer = product.Offer,
                Price = product.Price,
                SubCategoryId = product.SubCategoryId,
                ExistingImages = context.Images.Where(x => x.ProductId == id).ToList(),
                Stocks = product.Stocks.ToList()

            };

            return View("CreateProduct", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> EditProduct(AddProductViewModel product)
        {
            ViewBag.Departments = context.CategoriesSup.OrderBy(x => x.Name).ToList();

            if (!ModelState.IsValid)
                return RedirectToAction(nameof(CreateProduct),product);

            var existing = await context.Products.Include(p => p.Images).Include(x => x.Stocks).FirstOrDefaultAsync(x => x.Id == product.Id);

            if (existing == null)
                return NotFound();

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Offer = product.Offer;
            existing.Price = product.Price;
            existing.SubCategoryId = product.SubCategoryId;

            if (product.formFile != null)
            {
                foreach (var image in product.formFile)
                {
                    var img = new Images
                    {
                        ImageURL = UploadImage(image),
                        ProductId = existing.Id
                    };
                    context.Images.Add(img);
                }
            }

            //handleing stock
            var incomingStocks = product.Stocks;
            foreach (var exitstock in incomingStocks)
            {
                var exist = await context.Stocks.FirstOrDefaultAsync(x => x.StockId == exitstock.StockId);
                if (exist != null)
                {
                    exist.Color = exitstock.Color;
                    exist.Size = exitstock.Size;
                    exist.QuantityAvaiable = exitstock.QuantityAvaiable;
                } // new stock
                else
                {
                    existing.Stocks.Add(new Stock { Color = exitstock.Color, Size = exitstock.Size, QuantityAvaiable = exitstock.QuantityAvaiable });
                }
            }


            /*   // تحديث الموجود أو إضافته
               foreach (var stock in incomingStocks)
               {
                   if (!string.IsNullOrWhiteSpace(stock.Color) && stock.QuantityAvaiable.HasValue)
                   {
                       var matched = existingStocks.FirstOrDefault(s =>
                           s.Color == stock.Color && s.Size == stock.Size);

                       if (matched != null)
                       {
                           // تحديث الكمية فقط
                           matched.QuantityAvaiable = stock.QuantityAvaiable;
                           matched.LastUpdated = DateTime.Now;
                       }
                       else
                       {
                           // إدخال جديد
                           stock.ProductId = existing.Id;

                           stock.AddedDate = DateTime.Now;
                           context.Stocks.Add(stock);
                       }
                   }
               }

               // حذف المخازن التي لم تعد موجودة في النموذج
               foreach (var stockInDb in existingStocks)
               {
                   var stillExists = incomingStocks.Any(s =>
                       s.Color == stockInDb.Color && s.Size == stockInDb.Size);

                   if (!stillExists)
                   {
                       context.Stocks.Remove(stockInDb);
                   }
               }

              */
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(listProduct));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]

        public IActionResult DeleteProduct(int id)
        {
            var product = context.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            foreach (var image in product.Images)
            {
                DeleteImage(image.ImageURL);
            }

            context.Products.Remove(product);
            context.SaveChanges();
            return RedirectToAction(nameof(listProduct));
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]

        public IActionResult DeleteImageByid(string imageId)
        {
            var image = context.Images.FirstOrDefault(x => x.ImageId == imageId);
            if (image == null)
                return NotFound();

            var proid = image.ProductId;

            // حذف الصورة من المجلد
            DeleteImage(image.ImageURL);

            context.Images.Remove(image);
            context.SaveChanges();

            // توجيه إلى صفحة تعديل المنتج وإرسال المعرف كـ id
            return RedirectToAction("EditProduct", new { id = proid });
        }


        public IActionResult DetailsProduct(int ProductID)
        {
            // جلب المنتج الحالي مع القسم الفرعي
            var currentProduct = context.Products
                .Include(p => p.SubCategory)
                .ThenInclude(sc => sc.Category)
                .Include(x => x.Images)
                .Include(x => x.Stocks)
                .FirstOrDefault(p => p.Id == ProductID);

            if (currentProduct == null)
                return NotFound();

            var subCategoryId = currentProduct.SubCategoryId;

            //  بجيب لسته من المنتجات التابعه لنفس القسم للتنقل بين المنتجات بحسب موقع اندكس في اللسته
            var productsInSameSubCategory = context.Products
                .Where(p => p.SubCategoryId == subCategoryId)
                .OrderBy(p => p.Id)
                .ToList();

            // إيجاد موقع المنتج الحالي
            int currentIndex = productsInSameSubCategory.FindIndex(p => p.Id == ProductID);

            Product? previousProduct = currentIndex > 0 ? productsInSameSubCategory[currentIndex - 1] : null;
            Product? nextProduct = currentIndex < productsInSameSubCategory.Count - 1 ? productsInSameSubCategory[currentIndex + 1] : null;

            ViewBag.PreviousProduct = previousProduct;
            ViewBag.NextProduct = nextProduct;
            ViewBag.FuteareProducts = context.Products.Include(x => x.Images).Include(x=>x.Stocks)
                .OrderByDescending(x => x.Id).Take(12).ToList();
            ViewBag.RelationOrder = context.Products.Include(x => x.Images).Include(x=>x.Stocks)
                .Where(m => m.SubCategoryId == currentProduct.SubCategoryId)
                 .OrderByDescending(x => x.Id).Take(10).ToList();
            return View(currentProduct);
        }





    }
}
//public async Task<PartialViewResult> ViewSomeProductPartial(int page=1) 
//{

//    int PageSize = 6;
//    var totalProducts = await context.Products.CountAsync();
//    var totalPages = (int)Math.Ceiling(totalProducts / (double)PageSize);

//    var products = await context.Products
//        .OrderBy(p => p.Id)
//     //   .Where(x=>x.SubCategoryId==  )
//        .Skip((page - 1) * PageSize)
//        .Take(PageSize)
//        .ToListAsync();

//    var Hview = new HViewModel
//    {
//        Categories = context.Categories.ToList(),
//        SubCategories = context.CategoriesSup.ToList(),
//        Products = products,
//        CurrentPage = page,
//        TotalPages = totalPages,
//        PageSize = PageSize
//    };

//    return PartialView(Hview);


//}



//[HttpGet]
//public IActionResult SearchProducts(string term)
//{
//    var products = context.Products
//        .Where(p => p.Name.Contains(term))
//        .Select(p => new {
//            id = p.Id,
//            label = p.Name
//        })
//        .ToList();
//    return RedirectToAction("Category", "PruductCategory",products);
//}
