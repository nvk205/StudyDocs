using System;
using System.Drawing;
using System.Windows.Forms;
using StudyDocs.DAO;

namespace StudyDocs.Forms
{
    public class RegisterForm : Form
    {
        private TextBox txtUser, txtPass, txtConfirm, txtName;
        private Label lblStatus;

        public RegisterForm()
        {
            this.Text = "Đăng ký tài khoản mới";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            this.Controls.Add(new Label { Text = "Tài khoản (*):", Location = new Point(30, y), AutoSize = true });
            txtUser = new TextBox { Location = new Point(140, y - 3), Width = 200 };
            this.Controls.Add(txtUser);

            y += 40;
            this.Controls.Add(new Label { Text = "Tên hiển thị:", Location = new Point(30, y), AutoSize = true });
            txtName = new TextBox { Location = new Point(140, y - 3), Width = 200 };
            this.Controls.Add(txtName);

            y += 40;
            this.Controls.Add(new Label { Text = "Mật khẩu (*):", Location = new Point(30, y), AutoSize = true });
            txtPass = new TextBox { Location = new Point(140, y - 3), Width = 200, UseSystemPasswordChar = true };
            this.Controls.Add(txtPass);

            y += 40;
            this.Controls.Add(new Label { Text = "Nhập lại MK (*):", Location = new Point(30, y), AutoSize = true });
            txtConfirm = new TextBox { Location = new Point(140, y - 3), Width = 200, UseSystemPasswordChar = true };
            this.Controls.Add(txtConfirm);

            y += 50;
            var btnReg = new Button { Text = "Đăng ký", Location = new Point(140, y), Width = 90, Height = 30, BackColor = Color.ForestGreen, ForeColor = Color.White };
            var btnCancel = new Button { Text = "Hủy", Location = new Point(250, y), Width = 90, Height = 30 };
            this.Controls.AddRange(new Control[] { btnReg, btnCancel });

            y += 40;
            lblStatus = new Label { Location = new Point(30, y), Width = 310, ForeColor = Color.Red, TextAlign = ContentAlignment.MiddleCenter };
            this.Controls.Add(lblStatus);

            btnReg.Click += BtnReg_Click;
            btnCancel.Click += (s, e) => this.Close();
        }

        private void BtnReg_Click(object sender, EventArgs e)
        {
            string u = txtUser.Text.Trim();
            string p = txtPass.Text;
            string c = txtConfirm.Text;
            string n = txtName.Text.Trim();

            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
            {
                lblStatus.Text = "Tài khoản và mật khẩu không được để trống.";
                return;
            }
            if (p != c)
            {
                lblStatus.Text = "Mật khẩu nhập lại không khớp.";
                return;
            }

            try
            {
                if (UserDAO.IsUserExists(u))
                {
                    lblStatus.Text = "Tài khoản này đã tồn tại!";
                    return;
                }

                UserDAO.Register(u, p, n);
                MessageBox.Show("Đăng ký thành công! Hãy đăng nhập.", "Thông báo");
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
            }
        }
    }
}