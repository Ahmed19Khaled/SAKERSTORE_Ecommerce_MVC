using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopApp.Data;
using ShopApp.Models;

namespace ShopApp.ViewComponents
{
    public class InformationNavLoginViewComponent : ViewComponent
    {
        private readonly AppDBcontext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        public InformationNavLoginViewComponent(AppDBcontext context, UserManager<User> userManager,IHttpContextAccessor contextAccessor )
        {
            _context = context;
            _userManager = userManager;
            httpContextAccessor = contextAccessor;

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(UserClaimsPrincipal);

            var user = await _userManager.FindByIdAsync(userId);

                return View(user);

        }



    }
}
