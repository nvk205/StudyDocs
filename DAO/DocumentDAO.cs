using System;
using System.Data;
using System.Data.SqlClient;
using StudyDocs.DTO;

namespace StudyDocs.DAO
{
    public static class DocumentDAO
    {
        // 1. Lấy danh sách (KHÔNG lấy nội dung file để nhẹ máy)
        public static DataTable GetDocuments(string keyword, int? subjectId, string type, int? userId)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            {
                string sql = @"
                    SELECT d.DocumentId, d.SubjectId, d.Title, s.Name AS SubjectName, 
                           d.[Type], d.FileName, d.Notes, d.LastOpened, d.[Status], d.CreatedAt,
                           u.Username AS Uploader  -- <--- Lấy thêm cột này
                    FROM dbo.[Document] d
                    LEFT JOIN dbo.[Subject] s ON d.SubjectId = s.SubjectId
                    LEFT JOIN dbo.[User] u ON d.UserId = u.UserId -- <--- Nối bảng User
                    WHERE 1=1 ";

                if (!string.IsNullOrEmpty(keyword)) sql += " AND d.Title LIKE N'%' + @kw + N'%' ";
                if (subjectId.HasValue) sql += " AND d.SubjectId = @sid ";
                if (!string.IsNullOrEmpty(type) && type != "(Tất cả)") sql += " AND d.[Type] = @tp ";

                // Nếu có userId truyền vào thì chỉ hiện tài liệu của user đó
                if (userId.HasValue) sql += " AND d.UserId = @uid ";

                sql += " ORDER BY d.CreatedAt DESC";

                using (var cmd = new SqlCommand(sql, con))
                {
                    if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@kw", keyword);
                    if (subjectId.HasValue) cmd.Parameters.AddWithValue("@sid", subjectId.Value);
                    if (!string.IsNullOrEmpty(type) && type != "(Tất cả)") cmd.Parameters.AddWithValue("@tp", type);
                    if (userId.HasValue) cmd.Parameters.AddWithValue("@uid", userId.Value);

                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    return dt;
                }
            }
        }

        // 2. Lấy nội dung file (Chỉ gọi khi bấm Mở)
        public static Document GetFileContent(int docId)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("SELECT FileName, FileData FROM dbo.[Document] WHERE DocumentId = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", docId);
                con.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        return new Document
                        {
                            DocumentId = docId,
                            FileName = rd["FileName"] as string,
                            // Kiểm tra null trước khi ép kiểu
                            FileData = rd["FileData"] == DBNull.Value ? null : (byte[])rd["FileData"]
                        };
                    }
                    return null;
                }
            }
        }

        // 3. Thêm tài liệu mới (Có lưu FileData)
        public static void Insert(Document doc)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.[Document] 
                  (Title, SubjectId, [Type], FileData, FileName, Notes, [Status], UserId)
                VALUES 
                  (@t, @sid, @tp, @data, @fname, @note, @st, @uid)", con))
            {
                cmd.Parameters.AddWithValue("@t", doc.Title);
                // Xử lý nếu SubjectId null
                cmd.Parameters.AddWithValue("@sid", doc.SubjectId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@tp", doc.Type);

                // Xử lý lưu Binary (File)
                if (doc.FileData != null && doc.FileData.Length > 0)
                    cmd.Parameters.Add("@data", SqlDbType.VarBinary, -1).Value = doc.FileData;
                else
                    cmd.Parameters.Add("@data", SqlDbType.VarBinary, -1).Value = DBNull.Value;

                cmd.Parameters.AddWithValue("@fname", doc.FileName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@note", doc.Notes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@st", doc.Status); // Status là bool (bit)
                cmd.Parameters.AddWithValue("@uid", doc.UserId ?? (object)DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // 4. Sửa tài liệu
        public static void Update(Document doc)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand(@"
                UPDATE dbo.[Document]
                SET Title = @t,
                    SubjectId = @sid,
                    Notes = @note,
                    [Status] = @st,
                    -- Logic thông minh: Nếu có file mới (@data khác null) thì cập nhật, 
                    -- còn không thì giữ nguyên file cũ (FileData = FileData)
                    FileData = CASE WHEN @data IS NULL THEN FileData ELSE @data END,
                    FileName = CASE WHEN @fname IS NULL THEN FileName ELSE @fname END,
                    [Type] = CASE WHEN @tp IS NULL THEN [Type] ELSE @tp END
                WHERE DocumentId = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", doc.DocumentId);
                cmd.Parameters.AddWithValue("@t", doc.Title);
                cmd.Parameters.AddWithValue("@sid", doc.SubjectId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@note", doc.Notes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@st", doc.Status);

                // Xử lý file
                if (doc.FileData != null && doc.FileData.Length > 0)
                {
                    cmd.Parameters.Add("@data", SqlDbType.VarBinary, -1).Value = doc.FileData;
                    cmd.Parameters.AddWithValue("@fname", doc.FileName);
                    cmd.Parameters.AddWithValue("@tp", doc.Type);
                }
                else
                {
                    // Truyền NULL để SQL biết là không cần update cột này
                    cmd.Parameters.Add("@data", SqlDbType.VarBinary, -1).Value = DBNull.Value;
                    cmd.Parameters.AddWithValue("@fname", DBNull.Value);
                    cmd.Parameters.AddWithValue("@tp", DBNull.Value);
                }

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // 5. Xóa tài liệu
        public static void Delete(int docId)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("DELETE FROM dbo.[Document] WHERE DocumentId=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", docId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // 6. Cập nhật ngày mở cuối
        public static void UpdateLastOpened(int docId)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE dbo.[Document] SET LastOpened = GETDATE() WHERE DocumentId = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", docId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        // 7. Hàm lấy file Bất đồng bộ (Async) - Giúp không đơ máy
        public static async System.Threading.Tasks.Task<Document> GetFileContentAsync(int docId)
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand("SELECT FileName, FileData FROM dbo.[Document] WHERE DocumentId = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", docId);
                await con.OpenAsync(); // Mở kết nối async
                using (var rd = await cmd.ExecuteReaderAsync()) // Đọc dữ liệu async
                {
                    if (rd.Read())
                    {
                        return new Document
                        {
                            DocumentId = docId,
                            FileName = rd["FileName"] as string,
                            FileData = rd["FileData"] == DBNull.Value ? null : (byte[])rd["FileData"]
                        };
                    }
                    return null;
                }
            }
        }

        // 8. Hàm lấy thống kê cho biểu đồ (Môn học - Số lượng)
        public static DataTable GetStatsBySubject()
        {
            using (var con = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = new SqlCommand(@"
                SELECT s.Name, COUNT(d.DocumentId) as Count
                FROM dbo.[Subject] s
                LEFT JOIN dbo.[Document] d ON s.SubjectId = d.SubjectId
                GROUP BY s.Name", con))
            {
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }
    }
}