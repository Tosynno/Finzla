using Finzla.Domain.Entities;
using Finzla.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finzla.Api
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext db, IConfiguration config)
        {
            if (await db.AppUsers.AnyAsync()) return;

            var username  = config["Auth:SeedAdmin:Username"] ?? "admin";
            var email     = config["Auth:SeedAdmin:Email"]    ?? "admin@finzla.io";
            var password  = config["Auth:SeedAdmin:Password"] ?? "Admin@1234";
            var firstName = config["Auth:SeedAdmin:FirstName"] ?? "System";
            var lastName  = config["Auth:SeedAdmin:LastName"]  ?? "Admin";

            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var admin = AppUser.Create(username, email, hash, firstName, lastName);

            await db.AppUsers.AddAsync(admin);
            await db.SaveChangesAsync();

            Console.WriteLine($"[Seed] Default admin user created: {username}");
        }
    }
}
