using System;

namespace StudyDocs.DTO
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; } // 'Admin' hoặc 'User'
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}