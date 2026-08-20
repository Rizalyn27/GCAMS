using GCAMS.Controllers;
using GCAMS.Migrations;
using GCAMS.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace GCAMS.Data
{
    public static class SeedData
    {
        public static void EnsureTestAdmin(AppDbContext context)
        {
            const string testUsername = "admin@test.local";

            if (context.Users.Any(u => u.Username == testUsername))
                return;

            byte[] salt = UsersController.CreateSalt();
            byte[] hash = UsersController.HashPassword("Test1234!", salt);

            var admin = new Users
            {
                Username = testUsername,
                Salt = Convert.ToBase64String(salt),
                Password = Convert.ToBase64String(hash),
                Role = "Admin",
                IsActive = true,
                PasswordChange = true // true = no forced change, matches your ChangePass logic
            };

            context.Users.Add(admin);
            context.SaveChanges();
        }
    }
}