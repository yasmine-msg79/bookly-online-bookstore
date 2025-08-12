using BookStore.Data;
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

            // Add Connection String
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ✅ Use ApplicationUser, not IdentityUser
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

            // Redirect to Login page if unauthorized
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";  // ✅ Make sure this is correct for your Identity area
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Add Identity area routing
            app.MapRazorPages();

            app.Run();
        }
    }
}
