using System.Data;
using System.Data.SqlClient;
using StudyDocs.DTO;

namespace StudyDocs.DAO
{
    public static class SubjectDAO
    {
        // Lấy tất cả môn học
        public static DataTable GetAll()
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var da = new SqlDataAdapter("SELECT * FROM dbo.[Subject] ORDER BY [Name]", con))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Thêm môn học mới (Dành cho Admin sau này)
        public static void Insert(string name)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("INSERT INTO dbo.[Subject]([Name]) VALUES(@n)", con))
            {
                cmd.Parameters.AddWithValue("@n", name);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ... Các hàm cũ giữ nguyên ...

        public static void Delete(int subjectId)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("DELETE FROM dbo.[Subject] WHERE SubjectId=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", subjectId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}