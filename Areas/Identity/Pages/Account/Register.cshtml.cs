using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using BookStore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using BookStore.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;


namespace BookStore.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserStore<ApplicationUser> _userStore;


        public RegisterModel(
     UserManager<ApplicationUser> userManager,
     SignInManager<ApplicationUser> signInManager,
     IUserStore<ApplicationUser> userStore,
     IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _environment = environment;
        }


        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]

            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }


            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public Gender Gender { get; set; }

            public IFormFile? ProfileImage { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Email is already registered. Please use a different email.");
                    return Page();
                }

                string uniqueFileName = null;

                if (Input.ProfileImage != null && Input.ProfileImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images/profile");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.ProfileImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.ProfileImage.CopyToAsync(fileStream);
                    }
                }

                var user = new ApplicationUser
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    UserName = Input.UserName,
                    Email = Input.Email,
                    Gender = Input.Gender,
                    ProfileImageURL = uniqueFileName != null ? $"/images/profile/{uniqueFileName}" : null,
                    Role = Role.Guest
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToPage("~/Identity/Account/Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

    }
}
