using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Added to cart successfully!" });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int bookId)
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

            var itemCount = cart.CartItems.Sum(ci => ci.Quantity);
            var cartTotal = cart.CartItems.Sum(ci => ci.Book.Price * ci.Quantity);

            return Json(new
            {
                success = true,
                message = "Item removed.",
                itemCount = itemCount,
                cartTotal = cartTotal.ToString("C")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int bookId, int quantity)
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
                return await RemoveFromCart(bookId);
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

            // Recalculate
            var totalItems = cart.CartItems.Sum(ci => ci.Quantity);
            var cartTotal = cart.CartItems.Sum(ci => ci.Book.Price * ci.Quantity);

            return Json(new
            {
                success = true,
                quantity = item.Quantity,
                itemTotal = item.Book.Price * item.Quantity,
                cartTotal = cartTotal.ToString("C"),
                itemCount = totalItems
            });
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = await GetOrCreateCartAsync();
            
            if (cart == null || !cart.CartItems.Any())
            {
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
     string country,
     string addressOption = "new",
     int savedAddressIndex = 0,
     bool saveAddress = false)
        {
            // Get cart
            var cart = await GetOrCreateCartAsync();
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            // Validate stock
            var stockErrors = new List<string>();
            foreach (var item in cart.CartItems)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                if (book == null || book.StockQuantity < item.Quantity)
                {
                    stockErrors.Add($"{book?.Title ?? "Unknown Book"}: Only {book?.StockQuantity} in stock.");
                }
            }

            if (stockErrors.Any())
            {
                TempData["StockErrors"] = stockErrors;
                return RedirectToAction("Index");
            }

            // Get user
            var user = await _userManager.GetUserAsync(User);

            // Handle saved address
            if (user != null && addressOption == "saved")
            {
                var savedAddresses = await _context.Orders
                    .Where(o => o.UserId == user.Id)
                    .Select(o => new
                    {
                        o.ShippingAddress,
                        o.ShippingCity,
                        o.ShippingState,
                        o.ShippingZipCode,
                        o.ShippingCountry
                    })
                    .Distinct()
                    .ToListAsync();

                if (savedAddressIndex >= 0 && savedAddressIndex < savedAddresses.Count)
                {
                    var addr = savedAddresses[savedAddressIndex];
                    address = addr.ShippingAddress;
                    city = addr.ShippingCity;
                    state = addr.ShippingState;
                    zipCode = addr.ShippingZipCode;
                    country = addr.ShippingCountry;
                }
            }

            // Create order
            var order = new Order
            {
                UserId = user?.Id,
                Created = DateTime.Now,
                TotalAmount = cart.CartItems.Sum(ci => ci.Book.Price * ci.Quantity),
                ShippingFirstName = firstName,
                ShippingLastName = lastName,
                ShippingEmail = email,
                ShippingPhone = phone,
                ShippingAddress = address,
                ShippingCity = city,
                ShippingState = state,
                ShippingZipCode = zipCode,
                ShippingCountry = country
            };

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order items & update stock
                foreach (var item in cart.CartItems)
                {
                    var book = await _context.Books.FindAsync(item.BookId);
                    if (book != null)
                    {
                        book.StockQuantity -= item.Quantity;
                        _context.Books.Update(book);

                        _context.OrderItems.Add(new OrderItem
                        {
                            OrderId = order.Id,
                            BookId = item.BookId,
                            Quantity = item.Quantity,
                            Price = book.Price
                        });

                        _context.StockLogs.Add(new StockLog
                        {
                            BookId = book.Id,
                            QuantityChanged = -item.Quantity,
                            ChangeDate = DateTime.Now,
                            Reason = "Order placed",
                            OrderId = order.Id
                        });
                    }
                }

                // Clear the cart
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();

                // ✅ Delete old cookie
                Response.Cookies.Delete("CartId");

                // ✅ Create a new empty cart for future use (guests)
                var newCart = new Cart();
                _context.Carts.Add(newCart);
                await _context.SaveChangesAsync();
                if (stockErrors.Any())
                {
                    TempData["StockErrors"] = stockErrors;
                    return RedirectToAction("Index", "Cart"); // Show cart with errors
                }

                // ✅ Set new cookie
                Response.Cookies.Append("CartId", newCart.Id.ToString(), new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });

                // ✅ Redirect to confirmation
                return RedirectToAction("OrderConfirmed", new { id = order.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Checkout Error: {ex.Message}");
                TempData["Error"] = "An error occurred while processing your order.";
                return RedirectToAction("Index");
            }
        }

        private async Task<Cart> GetOrCreateCartAsync()
        {
            Cart cart = null;

            // Authenticated user
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    cart = await _context.Carts
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Book)
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);

                    if (cart != null) return cart;

                    // Create new cart
                    cart = new Cart { UserId = user.Id };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                    return cart;
                }
            }

            // Guest: use cookie
            var cartId = Request.Cookies["CartId"];
            if (!string.IsNullOrEmpty(cartId))
            {
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Book)
                    .FirstOrDefaultAsync(c => c.Id.ToString() == cartId);

                if (cart != null) return cart;
            }

            // ✅ Create new guest cart
            cart = new Cart();
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            Response.Cookies.Append("CartId", cart.Id.ToString(), new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

            return cart;
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

            return View(order);
        }
        public async Task<IActionResult> OrderHistory()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.Created)
                .ToListAsync();

            return View(orders);
        }
    }
}
