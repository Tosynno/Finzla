namespace Finzla.Domain.Entities
{
    public sealed class AppUser
    {
        private AppUser() { }

        public Guid Id { get; private set; }
        public string Username { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public string FirstName { get; private set; } = default!;
        public string LastName { get; private set; } = default!;
        public string? PhoneNumber { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        public static AppUser Create(
            string username, string email,
            string passwordHash, string firstName,
            string lastName, string? phoneNumber = null) =>
            new()
            {
                Id           = Guid.NewGuid(),
                Username     = username.Trim().ToLowerInvariant(),
                Email        = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                FirstName    = firstName.Trim(),
                LastName     = lastName.Trim(),
                PhoneNumber  = phoneNumber?.Trim(),
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow
            };

        public void RecordLogin()  => LastLoginAt = DateTime.UtcNow;
        public void Deactivate()   => IsActive = false;
        public void Activate()     => IsActive = true;
    }
}
