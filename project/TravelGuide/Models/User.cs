// Models/User.cs
namespace TravelGuide.Models
{
    public enum UserRole
    {
        Guest,
        User,
        Admin
    }

    public enum PlayerState
    {
        Idle,
        Playing,
        Paused,
        Stopped
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PreferredLanguage { get; set; } = "vi";
        public UserRole Role { get; set; } = UserRole.User;
        public bool IsActive { get; set; } = true;
    }
}
