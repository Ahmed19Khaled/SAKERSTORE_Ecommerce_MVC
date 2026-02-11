using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopApp.Data;
using ShopApp.Data.ShopApp.Data;
using ShopApp.Models;
using ShopApp.Models.Services;
using ShopApp.Services;

var builder = WebApplication.CreateBuilder(args);

// إضافة DbContext
builder.Services.AddDbContext<AppDBcontext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDb")));

// إضافة الهوية Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // هذه الإعدادات الافتراضية، نضيف الـ ClaimTypes
    options.ClaimsIdentity.UserNameClaimType = System.Security.Claims.ClaimTypes.Name;
    options.ClaimsIdentity.RoleClaimType = System.Security.Claims.ClaimTypes.Role;
})
.AddEntityFrameworkStores<AppDBcontext>()
.AddDefaultTokenProviders();


// paymob
builder.Services.AddScoped<PaymobService>();
// تسجيل خدمة الـ CartService
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<EmailSender>();

// إضافة MVC
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication()
   .AddGoogle(options =>
   {
       options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
       options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
       options.CallbackPath = "/signin-google"; // هذا مهم!
   })

   .AddFacebook(options =>
   {
         options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
         options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
         options.CallbackPath = "/signin-facebook"; // هذا مهم!

   });


builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
// إعدادات الادوار
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedRolesAndAdmin(services);
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ترتيب الـ Middleware الخاص بالهوية
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();