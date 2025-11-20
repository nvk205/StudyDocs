using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using StudyDocs.DAO;
using StudyDocs.DTO;
using StudyDocs.Services;
using System.Data; // Cần cái này để xử lý STT

namespace StudyDocs.Forms
{
    public class MainForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cboFilterSubject;
        private Button btnAdd, btnEdit, btnDelete, btnOpen, btnReload;
        private Button btnUserMenu;
        private ContextMenuStrip userContextMenu;
        private PictureBox loadingSpinner;

        public MainForm()
        {
            InitializeUI();
            LoadSubjectFilter();
            LoadData();
        }

        private void InitializeUI()
        {
            this.Text = "StudyDocs Pro - Hệ thống quản lý tài liệu";
            this.Size = new Size(1280, 750); // Tăng nhẹ chiều rộng để chứa hết các nút
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // ==================================================================================
            // 1. THANH CÔNG CỤ (TOP PANEL)
            // ==================================================================================
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.WhiteSmoke };

            // --- A. NÚT USER (GIỜ SẼ NẰM ĐẦU TIÊN BÊN TRÁI) ---

            // Tạo Menu thả xuống
            userContextMenu = new ContextMenuStrip();
            userContextMenu.Items.Add("👤  Thông tin cá nhân", null, (s, e) => new ProfileForm().ShowDialog());
            if (Session.IsAdmin)
            {
                var itemStats = userContextMenu.Items.Add("📊  Thống kê hệ thống", null, (s, e) => new StatsForm().ShowDialog());
                var itemUsers = userContextMenu.Items.Add("👥  Quản lý người dùng", null, (s, e) => new UserManageForm().ShowDialog());
            }
            userContextMenu.Items.Add(new ToolStripSeparator());
            var itemLogout = userContextMenu.Items.Add("🚪  Đăng xuất", null, (s, e) => Application.Restart());
            itemLogout.ForeColor = Color.Red;

            // Nút User
            btnUserMenu = new Button
            {
                Text = $"👤 {Session.CurrentUser.Username} ▼",
                Size = new Size(160, 35),
                Location = new Point(20, 23), // <--- VỊ TRÍ MỚI: Sát lề trái
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            btnUserMenu.FlatAppearance.BorderSize = 0;
            btnUserMenu.Click += (s, e) => userContextMenu.Show(btnUserMenu, 0, btnUserMenu.Height);

            // --- B. KHU VỰC TÌM KIẾM & LỌC (Dời sang phải 1 đoạn) ---
            // (Tọa độ cũ + 180px để nhường chỗ cho nút User)

            var lblSearch = new Label { Text = "Tìm kiếm:", Location = new Point(200, 28), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(260, 25), Width = 200, Font = new Font("Segoe UI", 10) };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(); };

            var lblFilter = new Label { Text = "Lọc Môn:", Location = new Point(480, 28), AutoSize = true };
            cboFilterSubject = new ComboBox { Location = new Point(540, 25), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

            // --- C. CÁC NÚT CHỨC NĂNG (Cũng dời sang phải) ---
            btnReload = CreateButton("🔍 Tìm", 720, 23, 80, Color.LightGray, Color.Black);
            btnAdd = CreateButton("➕ Thêm", 810, 23, 90, Color.Teal, Color.White);
            btnEdit = CreateButton("✏️ Sửa", 910, 23, 80, Color.Orange, Color.White);
            btnOpen = CreateButton("📂 Mở file", 1000, 23, 100, Color.White, Color.Black);
            btnDelete = CreateButton("❌ Xóa", 1110, 23, 80, Color.IndianRed, Color.White);

            // Loading Spinner
            loadingSpinner = new PictureBox
            {
                Image = SystemIcons.Information.ToBitmap(),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(30, 30),
                Location = new Point(680, 23), // Đặt giữa ô lọc và nút Tìm
                Visible = false
            };

            topPanel.Controls.AddRange(new Control[] {
                btnUserMenu, // Add nút User trước
                lblSearch, txtSearch, lblFilter, cboFilterSubject,
                btnReload, btnAdd, btnEdit, btnOpen, btnDelete, loadingSpinner
            });

            this.Controls.Add(topPanel);

            // ==================================================================================
            // 2. BẢNG DỮ LIỆU (GIỮ NGUYÊN NHƯ CŨ)
            // ==================================================================================
            var bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                RowTemplate = { Height = 35 },
                Font = new Font("Segoe UI", 10)
            };
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;

            bodyPanel.Controls.Add(dgv);
            this.Controls.Add(bodyPanel);
            bodyPanel.BringToFront();

            // ==================================================================================
            // 3. EVENTS (GIỮ NGUYÊN)
            // ==================================================================================
            btnAdd.Click += (s, e) =>
            {
                using (var f = new UpsertForm())
                {
                    var r = f.ShowDialog();
                    LoadSubjectFilter();
                    if (r == DialogResult.OK) LoadData();
                }
            };
            btnEdit.Click += BtnEdit_Click;
            btnReload.Click += (s, e) => LoadData();
            btnDelete.Click += BtnDelete_Click;
            btnOpen.Click += BtnOpen_Click;
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnOpen_Click(null, null); };
        }

        private Button CreateButton(string text, int x, int y, int w, Color bg, Color fg)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = w,
                Height = 30,
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadSubjectFilter()
        {
            var dt = SubjectDAO.GetAll();
            var row = dt.NewRow(); row["SubjectId"] = DBNull.Value; row["Name"] = "(Tất cả)";
            dt.Rows.InsertAt(row, 0);
            cboFilterSubject.DataSource = dt;
            cboFilterSubject.DisplayMember = "Name";
            cboFilterSubject.ValueMember = "SubjectId";
            cboFilterSubject.SelectedIndexChanged += (s, e) => LoadData();
        }

        private void LoadData()
        {
            string keyword = txtSearch.Text.Trim();
            int? subjectIdFilter = cboFilterSubject.SelectedValue as int?;
            if (cboFilterSubject.SelectedIndex == 0) subjectIdFilter = null;
            int? userIdToFilter = Session.IsAdmin ? (int?)null : Session.CurrentUser.UserId;

            // Lấy dữ liệu gốc từ DB
            var dt = DocumentDAO.GetDocuments(keyword, subjectIdFilter, null, userIdToFilter);

            // --- XỬ LÝ THÊM CỘT STT (SỐ THỨ TỰ) ---
            // 1. Thêm cột STT vào DataTable ở vị trí đầu tiên (index 0)
            DataColumn colSTT = new DataColumn("STT", typeof(int));
            dt.Columns.Add(colSTT);
            colSTT.SetOrdinal(0); // Đẩy lên đầu

            // 2. Điền số liệu 1, 2, 3...
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["STT"] = i + 1;
            }
            // ---------------------------------------

            dgv.DataSource = dt;

            // Ẩn cột không cần thiết và chỉnh độ rộng
            if (dgv.Columns["DocumentId"] != null) dgv.Columns["DocumentId"].Visible = false;
            if (dgv.Columns["SubjectId"] != null) dgv.Columns["SubjectId"].Visible = false;
            if (dgv.Columns["FileName"] != null) dgv.Columns["FileName"].Visible = false;
            if (dgv.Columns["FileData"] != null) dgv.Columns["FileData"].Visible = false;

            // Chỉnh độ rộng cột STT nhỏ thôi
            if (dgv.Columns["STT"] != null)
            {
                dgv.Columns["STT"].Width = 50;
                dgv.Columns["STT"].HeaderText = "#";
                dgv.Columns["STT"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            var row = dgv.CurrentRow;
            var doc = new Document
            {
                DocumentId = (int)row.Cells["DocumentId"].Value,
                Title = row.Cells["Title"].Value.ToString(),
                SubjectId = row.Cells["SubjectId"].Value == DBNull.Value ? (int?)null : (int)row.Cells["SubjectId"].Value,
                SubjectName = row.Cells["SubjectName"].Value.ToString(),
                Notes = row.Cells["Notes"].Value.ToString(),
                Status = (bool)row.Cells["Status"].Value,
                FileName = row.Cells["FileName"].Value?.ToString()
            };
            using (var f = new UpsertForm(doc))
            {
                var r = f.ShowDialog();
                LoadSubjectFilter();
                if (r == DialogResult.OK) LoadData();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            if (MessageBox.Show("Xóa tài liệu này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DocumentDAO.Delete((int)dgv.CurrentRow.Cells["DocumentId"].Value);
                LoadData();
            }
        }

        private async void BtnOpen_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            int id = (int)dgv.CurrentRow.Cells["DocumentId"].Value;

            this.Cursor = Cursors.WaitCursor;
            loadingSpinner.Visible = true;
            btnOpen.Enabled = false;
            btnOpen.Text = "⏳ ...";

            try
            {
                var doc = await DocumentDAO.GetFileContentAsync(id);

                if (doc == null || doc.FileData == null)
                {
                    MessageBox.Show("Lỗi: File rỗng!");
                    return;
                }

                string tempPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks + "_" + (doc.FileName ?? "temp.bin"));
                File.WriteAllBytes(tempPath, doc.FileData);
                Process.Start(tempPath);
                DocumentDAO.UpdateLastOpened(id);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            finally
            {
                this.Cursor = Cursors.Default;
                loadingSpinner.Visible = false;
                btnOpen.Enabled = true;
                btnOpen.Text = "📂 Mở file";
            }
        }
    }
}