namespace ShopApp.Data
{
    using global::ShopApp.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    namespace ShopApp.Data
    {
        public static class DbInitializer
        {
            public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roles = { "Admin", "User", "Manager" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                //  إضافة أدمن  
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                var adminEmail = "admin@site.com";
                var admin = await userManager.FindByEmailAsync(adminEmail);

                if (admin == null)
                {
                    var newAdmin = new User
                    {
                        UserName = "admin@site.com",
                        Email = "admin@site.com",
                        FullName = "Site Administrator"
                    };

                    var result = await userManager.CreateAsync(newAdmin, "Admin@123"); // كلمة مرور آمنة
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newAdmin, "Admin");
                    }
                }


            }
        }
    }

}
