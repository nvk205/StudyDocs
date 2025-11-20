using System;
using System.Drawing;
using System.Windows.Forms;
using StudyDocs.DAO;
using StudyDocs.Services;

namespace StudyDocs.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUser;
        private TextBox txtPass;
        private Label lblStatus;

        public LoginForm()
        {
            // 1. Cài đặt Form
            this.Text = "Đăng nhập hệ thống";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 2. Tạo các control
            var lblUser = new Label { Text = "Tài khoản:", Location = new Point(30, 30), AutoSize = true };
            txtUser = new TextBox { Location = new Point(120, 27), Width = 200 };

            var lblPass = new Label { Text = "Mật khẩu:", Location = new Point(30, 70), AutoSize = true };
            txtPass = new TextBox { Location = new Point(120, 67), Width = 200, UseSystemPasswordChar = true };

            var btnLogin = new Button { Text = "Đăng nhập", Location = new Point(120, 110), Width = 90, Height = 30, BackColor = Color.CornflowerBlue, ForeColor = Color.White };
            var btnExit = new Button { Text = "Thoát", Location = new Point(230, 110), Width = 90, Height = 30 };

            var lnkRegister = new LinkLabel { Text = "Chưa có tài khoản? Đăng ký ngay", Location = new Point(120, 150), AutoSize = true };

            lblStatus = new Label { Location = new Point(30, 180), Width = 340, ForeColor = Color.Red, TextAlign = ContentAlignment.MiddleCenter };

            // 3. Thêm vào Form
            this.Controls.AddRange(new Control[] { lblUser, txtUser, lblPass, txtPass, btnLogin, btnExit, lnkRegister, lblStatus });

            // 4. Gán sự kiện (Logic nằm ở đây)
            this.AcceptButton = btnLogin; // Bấm Enter là đăng nhập
            btnLogin.Click += BtnLogin_Click;
            btnExit.Click += (s, e) => Application.Exit();
            lnkRegister.Click += (s, e) =>
            {
                // Mở form đăng ký (Ẩn form login đi)
                this.Hide();
                using (var reg = new RegisterForm())
                {
                    reg.ShowDialog();
                }
                this.Show(); // Hiện lại khi đăng ký xong
            };
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string u = txtUser.Text.Trim();
            string p = txtPass.Text;

            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
            {
                lblStatus.Text = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }

            try
            {
                // 1. Tìm user trong DB
                var user = UserDAO.GetByUsername(u);

                // 2. Kiểm tra mật khẩu
                if (user != null && SecurityService.VerifyPassword(p, user.PasswordHash, user.PasswordSalt))
                {
                    // 3. Lưu vào Session và báo thành công
                    if (!user.IsActive)
                    {
                        lblStatus.Text = "Tài khoản này đã bị khóa!";
                        return;
                    }

                    Session.CurrentUser = user;
                    this.DialogResult = DialogResult.OK; // Báo OK để Program.cs biết
                    this.Close();
                }
                else
                {
                    lblStatus.Text = "Sai tài khoản hoặc mật khẩu!";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi kết nối: " + ex.Message;
            }
        }
    }
}