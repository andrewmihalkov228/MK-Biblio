using LibrarySystem.Models;
using Microsoft.AspNetCore.Identity;

public static class RoleInitializer
{
    public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> rolesManager)
    {
        if (!await rolesManager.RoleExistsAsync("admin"))
        {
            await rolesManager.CreateAsync(new IdentityRole("admin"));
        }
        if (!await rolesManager.RoleExistsAsync("user"))
        {
            await rolesManager.CreateAsync(new IdentityRole("user"));
        }
        if (!await rolesManager.RoleExistsAsync("librarian"))
        {
            await rolesManager.CreateAsync(new IdentityRole("librarian"));
        }
    }
}