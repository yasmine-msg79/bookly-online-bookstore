<<<<<<< HEAD
﻿using BookStore.Data;
=======
using BookStore.Data;
>>>>>>> origin/main
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add Connection String and configure EF Core with MySQL
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
<<<<<<< HEAD
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ✅ Use ApplicationUser, not IdentityUser
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            }).AddEntityFrameworkStores<ApplicationDbContext>();
=======
                options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 28))));

            // Configure Identity with Roles support
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
>>>>>>> origin/main

            // Configure cookie settings (redirect to login if unauthorized)
            builder.Services.ConfigureApplicationCookie(options =>
            {
<<<<<<< HEAD
                options.LoginPath = "/Identity/Account/Login";  // ✅ Make sure this is correct for your Identity area
            });

=======
                options.LoginPath = "/Account/Login";
            });

            // Add Razor Pages
            builder.Services.AddRazorPages();

            // Register UserSeeder service for DI
            builder.Services.AddScoped<UserSeeder>();

            // Build the app
>>>>>>> origin/main
            var app = builder.Build();

            // Seed the super admin user on startup
            using (var scope = app.Services.CreateScope())
            {
                var userSeeder = scope.ServiceProvider.GetRequiredService<UserSeeder>();
                await userSeeder.CreateSuperAdminAsync();
            }

            // Configure HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

<<<<<<< HEAD
            app.UseAuthentication();   // ✅ must be before UseAuthorization
            app.UseAuthorization();

            // Initialize database with super admin
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    
                    // Create super admin if it doesn't exist
                    var superAdminEmail = "admin@bookstore.com";
                    var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
                    
                    if (superAdmin == null)
                    {
                        superAdmin = new ApplicationUser
                        {
                            UserName = superAdminEmail,
                            Email = superAdminEmail,
                            EmailConfirmed = true,
                            FirstName = "Super",
                            LastName = "Admin",
                            ProfileImageURL = "https://ui-avatars.com/api/?name=Super+Admin",
                            Gender = Gender.Male,
                            PhoneNumber = "0000000000"
                        };
                        
                        var result = await userManager.CreateAsync(superAdmin, "Admin123!");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(superAdmin, "Admin");
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while creating super admin.");
                }
            }

=======
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
>>>>>>> origin/main
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Add Identity area routing
            app.MapRazorPages();

            app.Run();
        }
    }
}
