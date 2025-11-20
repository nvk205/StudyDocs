using System.Security.Cryptography;

namespace StudyDocs.Services
{
    public static class SecurityService
    {
        // Tạo Hash và Salt từ mật khẩu thường
        public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                salt = new byte[16];
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                hash = pbkdf2.GetBytes(32);
            }
        }

        // Kiểm tra mật khẩu nhập vào có đúng không
        public static bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                var test = pbkdf2.GetBytes(32);
                if (test.Length != hash.Length) return false;

                for (int i = 0; i < test.Length; i++)
                    if (test[i] != hash[i]) return false;
                return true;
            }
        }
    }
}