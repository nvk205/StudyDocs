using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting; // Thư viện biểu đồ
using StudyDocs.DAO;

namespace StudyDocs.Forms
{
    public class StatsForm : Form
    {
        public StatsForm()
        {
            this.Text = "Thống kê dữ liệu";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            var chart = new Chart { Dock = DockStyle.Fill };
            var area = new ChartArea("MainArea");
            chart.ChartAreas.Add(area);

            var series = new Series("Documents")
            {
                ChartType = SeriesChartType.Pie, // Biểu đồ tròn
                IsValueShownAsLabel = true
            };
            chart.Series.Add(series);

            // ... (code cũ) ...
            var title = new Title("Tỷ lệ tài liệu theo Môn học");
            title.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            chart.Titles.Add(title);

            this.Controls.Add(chart);

            try
            {
                var dt = DocumentDAO.GetStatsBySubject();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    string subject = row["Name"].ToString();
                    int count = Convert.ToInt32(row["Count"]);
                    if (count > 0)
                    {
                        int i = series.Points.AddXY(subject, count);

                        series.Points[i].Label = $"{subject} ({count})";
                        series.Points[i].LegendText = subject; // Hiện ở chú thích
                    }
                }
                // Thêm chú thích (Legend) bên cạnh cho đẹp
                chart.Legends.Add(new Legend("Legend1") { Docking = Docking.Right });
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load thống kê: " + ex.Message); }
        }
    }
}