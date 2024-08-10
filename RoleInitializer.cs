using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

public static class RoleInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        string[] roleNames = { "Admin", "User" };
        IdentityResult roleResult;

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Create the roles and seed them to the database
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create a super user who will maintain the web app
        var poweruser = new IdentityUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
        };

        string userPWD = "YourSecurePassword123!";
        var _user = await userManager.FindByEmailAsync("admin@example.com");

        if (_user == null)
        {
            var createPowerUser = await userManager.CreateAsync(poweruser, userPWD);
            if (createPowerUser.Succeeded)
            {
                // Assign the user to the "Admin" role
                await userManager.AddToRoleAsync(poweruser, "Admin");
            }
        }
    }
}
