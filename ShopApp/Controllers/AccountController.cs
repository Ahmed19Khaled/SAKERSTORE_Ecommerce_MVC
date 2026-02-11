using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Data;
using ShopApp.Models;
using ShopApp.Services;
using ShopApp.ViewModel; 
using ShopApp.ViewModels;

namespace ShopApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDBcontext _context;
        private readonly IWebHostEnvironment environment;
        private readonly EmailSender _emailSender;

        public AccountController(UserManager<User> userManager,
                                 SignInManager<User> signInManager,
                                 RoleManager<IdentityRole> roleManager,
                                 IWebHostEnvironment _environment,
                                AppDBcontext context ,
                                EmailSender emailSender)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            environment = _environment;
        }

        private string UploadImage(IFormFile file)
        {                                                         // مانشاء فولدر للتخزين
            string uploadsFolder = Path.Combine(environment.WebRootPath, "ImageProfile");
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


        // GET: /Account/Login 
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        [AutoValidateAntiforgeryToken] 
        public async Task<IActionResult> Login(LoginViewModel model )
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "البريد الإلكتروني غير صحيح");
                return View(model);
            }
            // check email confirmed
            //if (!await _userManager.IsEmailConfirmedAsync(user))
            //{
            //    ModelState.AddModelError("", "يرجى تأكيد بريدك الإلكتروني قبل تسجيل الدخول.");
            //    return View(model);
            //}

            var result = await _signInManager.PasswordSignInAsync(user, model.Password,model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var role = await _userManager.GetRolesAsync(user);
                if (role.Contains("User"))
                return RedirectToAction("Index", "Home");
                else
                   return RedirectToAction("Dashboard", "Admin");

            }
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            /*
                  if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // إضافة المستخدم إلى دور "User"
                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole("User"));

                await _userManager.AddToRoleAsync(user, "User");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
             */
            if (!ModelState.IsValid)
                return View(model);
            var user = new User
            {
                Email = model.Email,
                FullName = model.FullName,
                UserName = model.Email,
                CreatedAt =DateTime.Now
            };
            var usermanger= await _userManager.CreateAsync(user,model.Password);
            // Add User to Role 
            if (usermanger.Succeeded)
            {   if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole ("User"));
                await _userManager.AddToRoleAsync(user, "User");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");


            }
            foreach (var error in usermanger.Errors)
                ModelState.AddModelError("", error.Description);


            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound();

                // الحوار هنا
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["StatusMessage"] = "Password has been changed.";
                    return RedirectToAction(nameof(Manage));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet] 
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction("ForgotPasswordConfirmation");

            var tokene = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Action("ResetPassword", "Account", new
            {
                token = tokene,
                email = user.Email
            }, protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "إعادة تعيين كلمة المرور",
                         $"يرجى النقر على الرابط لإعادة تعيين كلمة المرور: <a href='{callbackUrl}'>إعادة تعيين</a>");

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public ActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return View("Error");

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }








        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var roleList = await _userManager.GetRolesAsync(user);
            var role = string.Join(",", roleList);

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(); // حل المشكلة هنا

            var model = new ProfUserViewModel
            {
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                City = user.City,
                Country = user.Country,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                CreatedAt = user.CreatedAt,
                Orders = orders,
                Role = role,
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Updateprofile () 
        {
            var userid = _userManager.GetUserId(User);
            var user=_context.Users.FirstOrDefault(x=>x.Id == userid);
         
            return View(new ProfUserViewModel { 
               BirthDate=user.BirthDate ,
               City  = user.City ,
               Country = user.Country ,
               CreatedAt=user.CreatedAt,
               Email=user.Email,
               FullName = user.FullName,
               Gender=user.Gender,
               PhoneNumber =user.PhoneNumber,
               ProfilePictureUrl = user.ProfilePictureUrl,
             });
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfUserViewModel model, IFormFile ProfileImage)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            // تحديث باقي البيانات
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.City = model.City;
            user.Country = model.Country;
            user.BirthDate = model.BirthDate;
            user.ProfilePictureUrl = UploadImage(ProfileImage);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }


        //  التسجيل عن طريق جوجل او فيسبوك
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction("Login");

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (signInResult.Succeeded)
                return RedirectToAction("Index", "Home");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? $"{info.ProviderKey}@{info.LoginProvider}.com";
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "Unknown";


            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FullName = name
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    ModelState.AddModelError("", "Happened Error on login ");
                    return RedirectToAction("Login");
                }
            }

            // ربط المستخدم بالمزود الخارجي
            var alreadyLinked = (await _userManager.GetLoginsAsync(user))
                .Any(login => login.LoginProvider == info.LoginProvider && login.ProviderKey == info.ProviderKey);

            if (!alreadyLinked)
            {
                var linkResult = await _userManager.AddLoginAsync(user, info);
                if (!linkResult.Succeeded)
                {

                    ModelState.AddModelError("", "Happened Error on login ");
                    return RedirectToAction("Login");
                }
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home");
        }


    }
}


/*
    public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction("Login");

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (signInResult.Succeeded)
                return RedirectToAction("Index", "Test");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? $"{info.ProviderKey}@{info.LoginProvider}.com";
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "Unknown";


            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = name
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    ModelState.AddModelError("", "Happened Error on login ");
                    return RedirectToAction("Login");
                }
            }

            // ربط المستخدم بالمزود الخارجي
            var alreadyLinked = (await _userManager.GetLoginsAsync(user))
                .Any(login => login.LoginProvider == info.LoginProvider && login.ProviderKey == info.ProviderKey);

            if (!alreadyLinked)
            {
                var linkResult = await _userManager.AddLoginAsync(user, info);
                if (!linkResult.Succeeded)
                {

                    ModelState.AddModelError("", "Happened Error on login ");
                    return RedirectToAction("Login");
                }
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Test");
        }

        #endregion

        #region ForgetPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        }
        public IActionResult ForgotPasswordConfirmation()
        {
            return View("ForgotPasswordConfirmation");
        }

        #endregion

        #region Reset Password
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        #endregion

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");

        }


        // remote
        public async Task<IActionResult> IsEmailExist(string Email)
        {
            var result = await _userManager.FindByEmailAsync(Email);
            if (result == null)
                return Json(true);
            return Json(false);

        } 
  
 
 */

