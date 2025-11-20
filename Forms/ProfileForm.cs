using System;
using System.Drawing;
using System.Windows.Forms;
using StudyDocs.DAO;
using StudyDocs.Services;

namespace StudyDocs.Forms
{
    public class ProfileForm : Form
    {
        private TextBox txtOldPass, txtNewPass, txtConfirm;

        public ProfileForm()
        {
            this.Text = "Thông tin cá nhân & Đổi mật khẩu";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lblInfo = new Label { Text = $"Tài khoản: {Session.CurrentUser.Username}\nTên hiển thị: {Session.CurrentUser.DisplayName}\nVai trò: {Session.CurrentUser.Role}", Location = new Point(20, 20), AutoSize = true, ForeColor = Color.Blue };
            this.Controls.Add(lblInfo);

            var grp = new GroupBox { Text = "Đổi mật khẩu", Location = new Point(20, 90), Width = 340, Height = 200 };

            int y = 30;
            grp.Controls.Add(new Label { Text = "Mật khẩu cũ:", Location = new Point(10, y), AutoSize = true });
            txtOldPass = new TextBox { Location = new Point(120, y - 3), Width = 200, UseSystemPasswordChar = true };
            grp.Controls.Add(txtOldPass);

            y += 40;
            grp.Controls.Add(new Label { Text = "Mật khẩu mới:", Location = new Point(10, y), AutoSize = true });
            txtNewPass = new TextBox { Location = new Point(120, y - 3), Width = 200, UseSystemPasswordChar = true };
            grp.Controls.Add(txtNewPass);

            y += 40;
            grp.Controls.Add(new Label { Text = "Nhập lại mới:", Location = new Point(10, y), AutoSize = true });
            txtConfirm = new TextBox { Location = new Point(120, y - 3), Width = 200, UseSystemPasswordChar = true };
            grp.Controls.Add(txtConfirm);

            y += 40;
            var btnSave = new Button { Text = "Lưu thay đổi", Location = new Point(120, y), Width = 200, Height = 30, BackColor = Color.Teal, ForeColor = Color.White };
            grp.Controls.Add(btnSave);

            this.Controls.Add(grp);

            btnSave.Click += BtnSave_Click;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtOldPass.Text) || string.IsNullOrEmpty(txtNewPass.Text)) return;

            if (txtNewPass.Text != txtConfirm.Text)
            {
                MessageBox.Show("Mật khẩu mới không khớp!");
                return;
            }

            // Check pass cũ từ Session (hoặc query lại DB cho chắc, ở đây check nhanh)
            if (!SecurityService.VerifyPassword(txtOldPass.Text, Session.CurrentUser.PasswordHash, Session.CurrentUser.PasswordSalt))
            {
                MessageBox.Show("Mật khẩu cũ không đúng!");
                return;
            }

            try
            {
                UserDAO.ChangePassword(Session.CurrentUser.UserId, txtNewPass.Text);
                MessageBox.Show("Đổi mật khẩu thành công! Vui lòng đăng nhập lại.");
                Application.Restart();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}