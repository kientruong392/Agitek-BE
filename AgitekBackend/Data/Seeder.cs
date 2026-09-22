using AgitekBackend.Common;
using AgitekBackend.Common.Utils;
using AgitekBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AgitekBackend.Data
{
    public static class Seeder
    {
        public static async Task SeedDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure the database is created/migrated before seeding
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync())
            {
                var defaultPasswordHash = AuthUtils.HashPassword("123123");
                var users = new List<User>();
                int userCounter = 1;

                void CreateUser(RoleName role, string usernamePrefix, int count, bool isGuest = false)
                {
                    for (int i = 1; i <= count; i++)
                    {
                        var user = new User
                        {
                            Role = role,
                            IsActive = true,
                            IsVerified = true,
                            Providers = new List<Provider> { Provider.Credentials },
                            Profile = new Profile
                            {
                                FullName = isGuest ? $"Guest User {i}" : $"{role} User {i}",
                                PhoneNumber = $"098765432{userCounter % 10}",
                                Gender = GenderType.Other,
                                DateOfBirth = new DateTime(1990, 1, 1).AddDays(userCounter),
                                AvatarUrl = "https://ui-avatars.com/api/?name=" + (isGuest ? "Guest" : role.ToString())
                            }
                        };

                        if (!isGuest)
                        {
                            user.Username = $"{usernamePrefix}{i}";
                            user.Email = $"{usernamePrefix}{i}@agitek.com";
                            user.PasswordHash = defaultPasswordHash;
                        }
                        else
                        {
                            user.Username = null;
                            user.Email = null;
                            user.PasswordHash = null;
                        }

                        users.Add(user);
                        userCounter++;
                    }
                }

                // 1 SuperAdmin
                CreateUser(RoleName.SuperAdmin, "superadmin", 1);
                
                // 3 Admins
                CreateUser(RoleName.Admin, "admin", 3);
                
                // 2 Managers
                CreateUser(RoleName.Manager, "manager", 2);
                
                // 2 Staff
                CreateUser(RoleName.Staff, "staff", 2);
                
                // 2 Delivery
                CreateUser(RoleName.Delivery, "delivery", 2);
                
                // 2 Customers
                CreateUser(RoleName.Customer, "customer", 2);
                
                // 2 Guests
                CreateUser(RoleName.Guest, "guest", 2, isGuest: true);

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
        }
    }
}
