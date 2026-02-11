using ShopApp.Models;

namespace ShopApp.ViewModel
{
    public class AllProductsDitals
    {
        public SubCategory? SubCategory { get; set; }
        public Category? Category { get; set; }
        public List<Product>products { get; set; }

    }
}
