using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace BookStore.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartSummaryViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int cartItemCount = 0;

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null)
                {
                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);

                    if (cart != null)
                    {
                        cartItemCount = cart.CartItems.Sum(item => item.Quantity); // Changed from .Count to .Sum
                    }
                }
            }
            else
            {
                var cartId = Request.Cookies["CartId"] ?? HttpContext.Session.GetString("CartId");
                if (!string.IsNullOrEmpty(cartId))
                {
                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.Id.ToString() == cartId);

                    if (cart != null)
                    {
                        cartItemCount = cart.CartItems.Sum(item => item.Quantity); // Changed from .Count to .Sum
                    }
                }
            }

            return View(cartItemCount);
        }
    }
}