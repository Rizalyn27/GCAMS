using GCAMS.Controllers;
using GCAMS.Models.Users;

namespace GCAMS.Data
{
    public static class SeedData
    {
        /// <summary>
        /// Guarantees the system always has at least one administrator who can sign in.
        ///
        /// This is a break-glass mechanism, not a convenience. It fires ONLY when no
        /// active admin exists — a fresh database, or one whose admins have all been
        /// wiped or deactivated. On every normal startup it does nothing, so it can
        /// never fight with accounts created through Users/Create.
        ///
        /// The old EnsureTestAdmin keyed off a hardcoded username instead, so it never
        /// noticed a database with zero usable admins as long as that one row existed
        /// in any state.
        /// </summary>
        public static void EnsureBootstrapAdmin(
            AppDbContext context,
            IConfiguration config,
            ILogger logger,
            bool isDevelopment)
        {
            // Nothing to do if somebody can already get in.
            if (context.Users.Any(u => u.Role == "Admin" && u.IsActive))
                return;

            var username = config["BootstrapAdmin:Username"];
            var password = config["BootstrapAdmin:Password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                if (!isDevelopment)
                {
                    // Refusing loudly beats silently creating a known-password admin
                    // on a real deployment.
                    logger.LogError(
                        "No active administrator exists and BootstrapAdmin:Username/Password " +
                        "are not configured. Set them (environment variables or user-secrets) " +
                        "and restart, or nobody will be able to sign in.");
                    return;
                }

                username = "admin";
                password = "Test1234!";
                logger.LogWarning(
                    "No active administrator found. Creating the development fallback '{Username}'. " +
                    "Set BootstrapAdmin:Username and BootstrapAdmin:Password to override.", username);
            }

            var existing = context.Users.FirstOrDefault(u => u.Username == username);

            byte[] salt = UsersController.CreateSalt();
            byte[] hash = UsersController.HashPassword(password, salt);

            if (existing != null)
            {
                // The account exists but is deactivated or demoted — revive it rather
                // than colliding on the username.
                existing.Salt = Convert.ToBase64String(salt);
                existing.Password = Convert.ToBase64String(hash);
                existing.Role = "Admin";
                existing.IsActive = true;
                existing.PasswordChange = false;   // false == must change at next sign-in

                logger.LogWarning("Reactivated '{Username}' as a bootstrap administrator.", username);
            }
            else
            {
                context.Users.Add(new Users
                {
                    Username = username,
                    Salt = Convert.ToBase64String(salt),
                    Password = Convert.ToBase64String(hash),
                    Role = "Admin",
                    IsActive = true,
                    // false is the forced-change flag Program.cs and ChangePass read.
                    // The bootstrap password is temporary by design.
                    PasswordChange = false
                });

                logger.LogWarning("Created bootstrap administrator '{Username}'.", username);
            }

            context.SaveChanges();
        }
    }
}