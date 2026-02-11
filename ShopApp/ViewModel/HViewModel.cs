using ShopApp.Models;

namespace ShopApp.ViewModel
{
    public class HViewModel
    {
        public List<Category> Categories { get; set; }
        public List<SubCategory> SubCategories { get; set; }
        public List<Product> Products { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
