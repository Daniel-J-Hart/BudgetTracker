namespace BudgetTracker.Models
{
    // Represents a user in the system, including their login credentials
    public class User
    {
        // The username that the user will use to log in
        public required string Username { get; set; }

        // The hashed password (for security, never store raw passwords)
        public required string PasswordHash { get; set; }
    }
}