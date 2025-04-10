using System;

namespace FlowerShopManagement
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }

        public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsManager => Role?.Equals("Manager", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsEmployee => Role?.Equals("Employee", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}