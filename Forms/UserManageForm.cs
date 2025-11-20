using System;
using System.Drawing;
using System.Windows.Forms;
using StudyDocs.DAO;
using StudyDocs.DTO; // Để dùng Session

namespace StudyDocs.Forms
{
    public class UserManageForm : Form
    {
        private DataGridView dgv;
        private Button btnLock, btnDelete;

        public UserManageForm()
        {
            this.Text = "Quản lý Người dùng (Admin Only)";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Grid
            dgv = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 380,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            this.Controls.Add(dgv);

            // Buttons
            btnLock = new Button { Text = "🔒 Khóa / Mở khóa", Location = new Point(20, 400), Width = 150, Height = 40, BackColor = Color.Orange, ForeColor = Color.White };
            btnDelete = new Button { Text = "❌ Xóa vĩnh viễn", Location = new Point(190, 400), Width = 150, Height = 40, BackColor = Color.Red, ForeColor = Color.White };

            this.Controls.AddRange(new Control[] { btnLock, btnDelete });

            LoadData();

            btnLock.Click += BtnLock_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        private void LoadData()
        {
            dgv.DataSource = UserDAO.GetAllUsers();
            if (dgv.Columns["PasswordHash"] != null) dgv.Columns["PasswordHash"].Visible = false; // Giấu cột mật khẩu đi
        }

        private void BtnLock_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            int id = (int)dgv.CurrentRow.Cells["UserId"].Value;
            string user = dgv.CurrentRow.Cells["Username"].Value.ToString();
            bool isActive = (bool)dgv.CurrentRow.Cells["IsActive"].Value;

            // Không cho khóa chính mình
            if (user == Services.Session.CurrentUser.Username) { MessageBox.Show("Không thể tự khóa chính mình!"); return; }

            string msg = isActive ? $"Bạn muốn KHÓA tài khoản '{user}'?" : $"Bạn muốn MỞ KHÓA tài khoản '{user}'?";
            if (MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                UserDAO.ToggleLock(id, isActive);
                LoadData();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            int id = (int)dgv.CurrentRow.Cells["UserId"].Value;
            string user = dgv.CurrentRow.Cells["Username"].Value.ToString();

            if (user == Services.Session.CurrentUser.Username) { MessageBox.Show("Không thể tự xóa chính mình!"); return; }

            if (MessageBox.Show($"CẢNH BÁO: Bạn có chắc muốn XÓA VĨNH VIỄN user '{user}'?\nTất cả tài liệu của họ sẽ mất chủ sở hữu.", "Nguy hiểm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                UserDAO.DeleteUser(id);
                LoadData();
            }
        }
    }
}