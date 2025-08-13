using BookStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: Users/Suspend/{id}
    [HttpPost]
    public async Task<IActionResult> Suspend(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest();

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        // Toggle suspension status
        user.IsSuspended = !user.IsSuspended;

        _context.Update(user);
        await _context.SaveChangesAsync();

        // Redirect back to dashboard or users list
        return RedirectToAction("Dashboard", "Dashboard");
    }
}
