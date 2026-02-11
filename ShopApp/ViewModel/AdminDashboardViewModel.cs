using ShopApp.Models;

namespace ShopApp.ViewModel
{
    public class AdminDashboardViewModel
    {
        public int UsersCount { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; } // إن أردت إضافته
        public int TotalVisitors { get; set; }     // إن أردت إضافته
        public int ordertoday { get; set; }

        public List<Order> LatestOrders { get; set; }
        public List<TopProductViewModel> TopProducts { get; set; }
        public List<Stock> LowStockProducts { get; set; }
        public List<MonthlyIncomeData> MonthlyIncome { get; set; }
        public User User { get; set; } // معلومات المستخدم الحالي
        public int ProductsCount { get; set; }
        public int Followers { get; set; }
        public string imageuser { get; set; } // صورة المستخدم إن أردت إضافتها  
    }
    public class MonthlyIncomeData
    {
        public string Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class TopProductViewModel
    {
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
        public int TotalQTY { get; set; }
        public string ImageUrl { get; set; }
        public bool isavilabel { get; set; }
        public string Category { get; set; }
        public DateTime SaleDate { get; set; } // إذا كان ضروريًا
        public decimal Price { get; set; }
    }

}
