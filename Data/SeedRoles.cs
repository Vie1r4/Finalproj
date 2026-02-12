using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Data
{
    public static class SeedRoles
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "Armazém", "Técnico", "Comercial" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Se existir pelo menos um utilizador e nenhum for Admin, atribuir Admin ao primeiro (para poder aceder ao painel).
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var users = userManager.Users.ToList();
            if (users.Count > 0)
            {
                var algumAdmin = await userManager.GetUsersInRoleAsync("Admin");
                if (algumAdmin.Count == 0)
                {
                    await userManager.AddToRoleAsync(users[0], "Admin");
                }
            }
        }
    }
}
