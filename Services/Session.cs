using StudyDocs.DTO;

namespace StudyDocs.Services
{
    public static class Session
    {
        // Biến toàn cục lưu người dùng đang đăng nhập
        public static User CurrentUser { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static bool IsAdmin => CurrentUser != null && CurrentUser.Role == "Admin";
    }
}