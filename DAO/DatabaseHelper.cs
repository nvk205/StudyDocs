using System.Configuration;

namespace StudyDocs.DAO
{
    public static class DatabaseHelper
    {
        // Lấy chuỗi kết nối từ App.config
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["StudyDocsDb"].ConnectionString;
    }
}