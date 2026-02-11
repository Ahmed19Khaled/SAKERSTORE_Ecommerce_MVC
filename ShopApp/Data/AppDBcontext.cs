using System;
using ShopApp.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
 using ShopApp.Models;
namespace ShopApp.Data
{
    public class AppDBcontext : IdentityDbContext<User>
    {


        public AppDBcontext(DbContextOptions<AppDBcontext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get;   set; }
        public DbSet<SubCategory> CategoriesSup { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }   
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Images> Images { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // عند حذف Category → احذف SubCategories
            modelBuilder.Entity<SubCategory>()
                .HasOne(s => s.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // عند حذف SubCategory → احذف Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.SubCategory)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Stock)
                .WithMany()
                .HasForeignKey(c => c.StockID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
