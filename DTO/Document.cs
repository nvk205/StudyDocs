using System;

namespace StudyDocs.DTO
{
    public class Document
    {
        public int DocumentId { get; set; }
        public string Title { get; set; }
        public int? SubjectId { get; set; }

        // Các trường hiển thị phụ
        public string SubjectName { get; set; }

        public string Type { get; set; } // pdf, docx...

        // Dữ liệu file (Quan trọng)
        public byte[] FileData { get; set; }
        public string FileName { get; set; }

        public string Notes { get; set; }
        public bool Status { get; set; }
        public DateTime? LastOpened { get; set; }
        public DateTime CreatedAt { get; set; }

        // --- ĐÂY LÀ DÒNG BẠN ĐANG THIẾU ---
        public int? UserId { get; set; }
    }
}