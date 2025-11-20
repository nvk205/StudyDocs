using StudyDocs.DTO;
using StudyDocs.Services;
using System;
using System.Data;
using System.Data.SqlClient;

namespace StudyDocs.DAO
{
    public static class UserDAO
    {
        // Tìm user theo tên đăng nhập (Dùng cho Login)
        public static User GetByUsername(string username)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("SELECT * FROM dbo.[User] WHERE Username = @u", con))
            {
                cmd.Parameters.AddWithValue("@u", username);
                con.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;

                    return new User
                    {
                        UserId = (int)rd["UserId"],
                        Username = (string)rd["Username"],
                        PasswordHash = (byte[])rd["PasswordHash"],
                        PasswordSalt = (byte[])rd["PasswordSalt"],
                        DisplayName = rd["DisplayName"] as string,
                        Role = (string)rd["Role"],
                        IsActive = (bool)rd["IsActive"]
                    };
                }
            }
        }

        // Đăng ký tài khoản mới
        public static void Register(string username, string password, string displayName)
        {
            // 1. Tạo mã hóa mật khẩu
            SecurityService.CreatePasswordHash(password, out var hash, out var salt);

            // 2. Lưu vào SQL
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.[User] (Username, PasswordHash, PasswordSalt, DisplayName, Role, IsActive)
                VALUES (@u, @h, @s, @d, 'User', 1)", con))
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@h", hash);
                cmd.Parameters.AddWithValue("@s", salt);
                cmd.Parameters.AddWithValue("@d", displayName ?? (object)DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Kiểm tra user có tồn tại không (để tránh đăng ký trùng)
        public static bool IsUserExists(string username)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.[User] WHERE Username = @u", con))
            {
                cmd.Parameters.AddWithValue("@u", username);
                con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
        public static void ChangePassword(int userId, string newPassword)
        {
            SecurityService.CreatePasswordHash(newPassword, out var hash, out var salt);

            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE dbo.[User] SET PasswordHash=@h, PasswordSalt=@s WHERE UserId=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.Add("@h", SqlDbType.VarBinary, 64).Value = hash;
                cmd.Parameters.Add("@s", SqlDbType.VarBinary, 16).Value = salt;
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Lấy danh sách tất cả user (trừ Admin ra để tránh xóa nhầm chính mình)
        public static DataTable GetAllUsers()
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var da = new SqlDataAdapter("SELECT UserId, Username, DisplayName, Role, IsActive, CreatedAt FROM dbo.[User] ORDER BY Role, Username", con))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Khóa / Mở khóa tài khoản
        public static void ToggleLock(int userId, bool currentStatus)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE dbo.[User] SET IsActive=@s WHERE UserId=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@s", !currentStatus); // Đảo ngược trạng thái
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Xóa tài khoản
        public static void DeleteUser(int userId)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("DELETE FROM dbo.[User] WHERE UserId=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}