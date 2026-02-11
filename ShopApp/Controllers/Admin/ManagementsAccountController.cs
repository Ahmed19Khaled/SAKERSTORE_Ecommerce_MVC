using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopApp.Models;

namespace ShopApp.Controllers.Admin
{
    [Authorize(Roles = "Manager")]
    public class ManagementsAccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManagementsAccountController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> AllAccount(string? Email, string? phone, string? role)
        {
            // استرجع كل المستخدمين مبدئياً
            var users = _userManager.Users.ToList();

            // الفلترة حسب الاسم أو الإيميل
            if (!string.IsNullOrWhiteSpace(Email))
            {
                users = users.Where(u =>
                    u.Email.Contains(Email, StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.Contains(Email, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // الفلترة حسب رقم الهاتف
            if (!string.IsNullOrWhiteSpace(phone))
            {
                users = users.Where(u =>
                    u.PhoneNumber != null && u.PhoneNumber.Contains(phone)
                ).ToList();
            }

            // الفلترة حسب الدور
            if (!string.IsNullOrWhiteSpace(role))
            {
                // الحصول على مستخدمين يملكون هذا الدور فقط
                var usersWithRole = new List<ShopApp.Models.User>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains(role))
                    {
                        usersWithRole.Add(user);
                    }
                }

                users = usersWithRole;
            }

            // تحميل الأدوار
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            return View(users);
        }



        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var currentRoles = await _userManager.GetRolesAsync(user);

            if (user != null)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);
            }

            return RedirectToAction("AllAccount");
        }
    }
}
