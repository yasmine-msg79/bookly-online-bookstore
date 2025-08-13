using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace BookStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCartAsync();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            try
            {
                var cart = await GetOrCreateCartAsync();

                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.BookId == bookId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.CartItems.Add(new CartItem
                    {
                        CartId = cart.Id,
                        BookId = bookId,
                        Quantity = quantity
                    });
                }

                await _context.SaveChangesAsync();

                // Get updated cart item count
                var cartItemCount = await GetCartItemCountAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = "Added to cart successfully!",
                        cartItemCount = cartItemCount
                    });
                }

                // Get the referring URL or default to Books/Index
                var returnUrl = Request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(returnUrl) || returnUrl.Contains("/Cart/"))
                {
                    returnUrl = Url.Action("Index", "Books");
                }

                // Add success message
                TempData["Success"] = "Book added to cart successfully!";

                // Redirect back to the referring page
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error adding item to cart: " + ex.Message
                    });
                }

                TempData["Error"] = "Error adding item to cart";
                var returnUrl = Request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = Url.Action("Index", "Books");
                }
                return Redirect(returnUrl);
            }
        }

        // Helper method to get cart item count
        private async Task<int> GetCartItemCountAsync()
        {
            var cart = await GetOrCreateCartAsync();
            return cart?.CartItems?.Sum(item => item.Quantity) ?? 0;
        }

        // Optional: Standalone endpoint to get cart count (useful for updating cart counter on page load)
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var cartItemCount = await GetCartItemCountAsync();
                return Json(new { success = true, count = cartItemCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                if (cart == null)
                {
                    return Json(new { success = false, message = "Cart not found." });
                }

                var item = cart.CartItems.FirstOrDefault(ci => ci.BookId == bookId);
                if (item == null)
                {
                    return Json(new { success = false, message = "Item not found." });
                }

                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int bookId, int quantity)
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                if (cart == null)
                {
                    return Json(new { success = false, message = "Cart not found." });
                }

                var item = cart.CartItems.FirstOrDefault(ci => ci.BookId == bookId);
                if (item == null)
                {
                    return Json(new { success = false, message = "Item not found." });
                }

                if (quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }

                // Check stock
                var book = await _context.Books.FindAsync(bookId);
                if (book == null || book.StockQuantity < quantity)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Only {book?.StockQuantity} available."
                    });
                }

                item.Quantity = quantity;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetCartDropdownContent()
        {
            var cart = await GetOrCreateCartAsync();

            if (cart == null || !cart.CartItems.Any())
            {
                return Content("<div class='text-center py-3'>Your cart is empty</div>");
            }

            var html = new StringBuilder();

            foreach (var item in cart.CartItems)
            {
                html.Append($@"
        <div class='dropdown-item'>
            <div class='d-flex justify-content-between'>
                <div>
                    <h6>{item.Book.Title}</h6>
                    <small>{item.Quantity} × ${item.Book.Price:F2}</small>
                </div>
                <div>
                    <button class='btn btn-sm btn-outline-danger remove-from-cart' 
                            data-book-id='{item.BookId}'>
                        <i class='fas fa-trash'></i>
                    </button>
                </div>
            </div>
        </div>");
            }

            html.Append($@"
    <div class='dropdown-divider'></div>
    <div class='dropdown-item'>
        <div class='d-flex justify-content-between'>
            <strong>Total:</strong>
            <strong>${cart.CartItems.Sum(i => i.Quantity * i.Book.Price):F2}</strong>
        </div>
    </div>
    <div class='dropdown-item'>
        <a href='{Url.Action("Index", "Cart")}' class='btn btn-primary btn-sm w-100'>View Cart</a>
    </div>");

            return Content(html.ToString());
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = await GetOrCreateCartAsync();

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            // Validate stock
            var stockValidationErrors = new List<string>();
            foreach (var item in cart.CartItems)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                if (book != null && book.StockQuantity < item.Quantity)
                {
                    stockValidationErrors.Add($"{book.Title} - Only {book.StockQuantity} available, requested {item.Quantity}");
                }
            }

            if (stockValidationErrors.Any())
            {
                TempData["StockErrors"] = stockValidationErrors;
                return RedirectToAction(nameof(Index));
            }

            // Pre-populate user data if logged in
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.UserEmail = user.Email;
                ViewBag.UserFirstName = user.FirstName; // Assuming these properties exist
                ViewBag.UserLastName = user.LastName;
                ViewBag.UserPhone = user.PhoneNumber;
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(
            string firstName,
            string lastName,
            string email,
            string phone,
            string address,
            string city,
            string state,
            string zipCode,
            string country)
        {
            var currentCart = await GetOrCreateCartAsync();
            if (currentCart == null || !currentCart.CartItems.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            // 🔴 SIMPLE: Check if essential fields are empty
            if (string.IsNullOrWhiteSpace(address))
            {
                TempData["Error"] = "⚠️ You can't leave the shipping address empty.";
                ViewBag.UserFirstName = firstName;
                ViewBag.UserLastName = lastName;
                ViewBag.UserEmail = email;
                ViewBag.UserPhone = phone;
                return View("Checkout", currentCart);
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                TempData["Error"] = "⚠️ Please enter your full name.";
                ViewBag.UserEmail = email;
                ViewBag.UserPhone = phone;
                return View("Checkout", currentCart);
            }

            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(zipCode))
            {
                TempData["Error"] = "⚠️ Please fill in city, state, and zip code.";
                ViewBag.UserFirstName = firstName;
                ViewBag.UserLastName = lastName;
                ViewBag.UserEmail = email;
                ViewBag.UserPhone = phone;
                return View("Checkout", currentCart);
            }

            // Optional: Set country default
            if (string.IsNullOrWhiteSpace(country))
                country = "Unknown";

            // ✅ All good — proceed to create order
            var order = new Order
            {
                UserId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : null,
                Created = DateTime.Now,

                ShippingFirstName = firstName,
                ShippingLastName = lastName,
                ShippingEmail = email,
                ShippingPhone = phone,
                ShippingAddress = address,
                ShippingCity = city,
                ShippingState = state,
                ShippingZipCode = zipCode,
                ShippingCountry = country,

                TotalAmount = currentCart.CartItems.Sum(ci => ci.Book.Price * ci.Quantity),
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in currentCart.CartItems)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                if (book != null)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        Price = book.Price
                    });
                    book.StockQuantity -= item.Quantity;
                }
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(currentCart.CartItems);
            _context.Carts.Remove(currentCart);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "🎉 Order placed successfully!";
                return RedirectToAction("OrderConfirmed", new { id = order.Id });
            }
            catch
            {
                TempData["Error"] = "❌ An error occurred while saving your order.";
                return View("Checkout", currentCart);
            }
        }


        public async Task<IActionResult> OrderConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Security check - only show order to the user who placed it or admin
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                if (order.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }
            }
            else if (!string.IsNullOrEmpty(order.UserId))
            {
                // Guest trying to access a user's order
                return Forbid();
            }

            return View(order);
        }

        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                // Should not happen if [Authorize] is used, but safe to check
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Created)
                .ToListAsync();

            return View(orders);
        }

        private async Task<Cart> GetOrCreateCartAsync()
        {
            Cart cart;

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Book)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Handle guest cart using session
                var cartId = HttpContext.Session.GetString("CartId");
                if (!string.IsNullOrEmpty(cartId) && int.TryParse(cartId, out int id))
                {
                    cart = await _context.Carts
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Book)
                        .FirstOrDefaultAsync(c => c.Id == id);
                }
                else
                {
                    cart = null;
                }

                if (cart == null)
                {
                    cart = new Cart { UserId = null }; // Guest cart
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("CartId", cart.Id.ToString());
                }
            }

            return cart;
        }
    }
}