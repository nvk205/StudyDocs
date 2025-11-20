using System;
using System.Drawing;
using System.Windows.Forms;
using StudyDocs.DAO;

namespace StudyDocs.Forms
{
    public class SubjectForm : Form
    {
        private ListBox lstSubjects;
        private TextBox txtName;
        private Button btnAdd, btnDelete;

        public SubjectForm()
        {
            this.Text = "Quản lý Môn học";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 1. Danh sách môn bên trái
            var lblList = new Label { Text = "Danh sách môn hiện có:", Location = new Point(10, 10), AutoSize = true };
            lstSubjects = new ListBox { Location = new Point(10, 35), Width = 200, Height = 260 };

            // 2. Khu vực thêm/xóa bên phải
            var grp = new GroupBox { Text = "Chức năng", Location = new Point(220, 30), Width = 200, Height = 265 };

            var lblName = new Label { Text = "Tên môn mới:", Location = new Point(10, 30), AutoSize = true };
            txtName = new TextBox { Location = new Point(10, 55), Width = 180 };

            btnAdd = new Button { Text = "Thêm Môn", Location = new Point(10, 90), Width = 180, Height = 35, BackColor = Color.Teal, ForeColor = Color.White };

            var lblNote = new Label { Text = "Chọn môn bên trái để xóa:", Location = new Point(10, 160), AutoSize = true, ForeColor = Color.Gray };
            btnDelete = new Button { Text = "Xóa Môn Đang Chọn", Location = new Point(10, 185), Width = 180, Height = 35, BackColor = Color.IndianRed, ForeColor = Color.White };

            grp.Controls.AddRange(new Control[] { lblName, txtName, btnAdd, lblNote, btnDelete });

            this.Controls.AddRange(new Control[] { lblList, lstSubjects, grp });

            // Load dữ liệu
            LoadList();

            // Sự kiện
            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        private void LoadList()
        {
            lstSubjects.DataSource = SubjectDAO.GetAll();
            lstSubjects.DisplayMember = "Name";
            lstSubjects.ValueMember = "SubjectId";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Nhập tên môn đi bạn ơi!"); return; }
            try
            {
                SubjectDAO.Insert(txtName.Text.Trim());
                txtName.Clear();
                LoadList(); // Load lại danh sách ngay
                this.DialogResult = DialogResult.OK; // Đánh dấu là có thay đổi dữ liệu
            }
            catch (Exception ex) { MessageBox.Show("Lỗi thêm: " + ex.Message); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lstSubjects.SelectedValue == null) { MessageBox.Show("Vui lòng chọn môn cần xóa ở danh sách bên trái!"); return; }

            if (MessageBox.Show("Bạn chắc chắn xóa môn này?\n(Các tài liệu thuộc môn này sẽ mất nhãn môn học)", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int id = (int)lstSubjects.SelectedValue;
                    SubjectDAO.Delete(id);
                    LoadList();
                    this.DialogResult = DialogResult.OK;
                }
                catch (Exception ex) { MessageBox.Show("Lỗi xóa: " + ex.Message); }
            }
        }
    }
}