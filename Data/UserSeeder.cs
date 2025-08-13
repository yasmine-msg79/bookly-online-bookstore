using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

public class UserSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserSeeder(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task CreateSuperAdminAsync()
    {
        string superAdminRole = Role.SuperAdmin.ToString();
        string adminRole = Role.Admin.ToString();
        string guestRole = Role.Guest.ToString();

        string superAdminEmail = "hanamasood74@gmail.com";
        var user = await _userManager.FindByEmailAsync(superAdminEmail);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = "hana.s",
                Email = superAdminEmail,
                FirstName = "Hana",
                LastName = "Salah",
                EmailConfirmed = true,
                PhoneNumber = "01060790080",
                PhoneNumberConfirmed = true,
                Gender = Gender.Female,
                Role = Role.SuperAdmin
            };

            string superAdminPassword = "SuperAdmin_123456789";

            var createUserResult = await _userManager.CreateAsync(user, superAdminPassword);
            if (!createUserResult.Succeeded)
            {
                foreach (var error in createUserResult.Errors)
                {
                    Console.WriteLine($"Error creating super admin user: {error.Description}");
                }
                return;
            }
        }

        if (!await _userManager.IsInRoleAsync(user, superAdminRole))
        {
            await _userManager.AddToRoleAsync(user, superAdminRole);
        }
    }
}