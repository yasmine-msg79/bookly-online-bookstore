using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookStore.Models; // your ApplicationUser namespace
using BookStore.ViewModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BookStore.Areas.Identity.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileModel(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        [BindProperty]
        public UserProfileViewModel Input { get; set; }

        // Bind file upload separately (not part of Input/ViewModel)
        [BindProperty]
        public IFormFile? ProfileImageFile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            Input = new UserProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImageUrl = user.ProfileImageURL
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                //foreach (var modelState in ModelState.Values)
                //{
                //    foreach (var error in modelState.Errors)
                //    {
                //        Console.WriteLine($"Validation error: {error.ErrorMessage}");
                //    }
                //}
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            // Update names
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;

            // Handle profile image upload if a file was submitted
            if (ProfileImageFile != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "profile");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImageFile.CopyToAsync(fileStream);
                }

                user.ProfileImageURL = uniqueFileName != null ? $"/images/profile/{uniqueFileName}" : null;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            // Reload the page to show updated info
            return RedirectToPage();
        }
    }
}
