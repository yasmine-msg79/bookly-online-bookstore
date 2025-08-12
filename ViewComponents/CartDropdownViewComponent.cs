using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;  // ✅ Required for User to be recognized as ClaimsPrincipal

namespace BookStore.ViewComponents
{
    public class CartDropdownViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartDropdownViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = await GetCartAsync();

            if (cart == null || !cart.CartItems.Any())
            {
                return View("EmptyCart");
            }

            return View(cart);
        }

        private async Task<Cart> GetCartAsync()
        {
            // Authenticated user
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null)
                {
                    return await _context.Carts
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Book)
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);
                }
            }
            else
            {
                // Guest user: check cookie
                var cartId = Request.Cookies["CartId"];
                if (!string.IsNullOrEmpty(cartId))
                {
                    return await _context.Carts
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Book)
                        .FirstOrDefaultAsync(c => c.Id.ToString() == cartId);
                }
            }

            return null;
        }
    }
}