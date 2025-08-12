using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BookStore.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "SuperAdmin")]
    public class AddAdminModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AddAdminModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }
            [Required]
            public string UserName { get; set; }

            [Required]
            public Gender Gender { get; set; }

            [Phone]
            public string PhoneNumber { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Email is already registered. Please use a different email.");
                return Page();
            }

            if (!await _roleManager.RoleExistsAsync(Role.Admin.ToString()))
            {
                await _roleManager.CreateAsync(new IdentityRole(Role.Admin.ToString()));
            }

            var user = new ApplicationUser
            {
                UserName = Input.UserName,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                EmailConfirmed = true,
                PhoneNumber = Input.PhoneNumber,
                PhoneNumberConfirmed = true,
                Gender = Input.Gender,
                Role = Role.Admin
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _userManager.AddToRoleAsync(user, Role.Admin.ToString());

            TempData["SuccessMessage"] = "Admin user created successfully!";
            return RedirectToPage();
        }
    }
}
