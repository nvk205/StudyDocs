using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using StudyDocs.DAO;
using StudyDocs.DTO;
using StudyDocs.Services;

namespace StudyDocs.Forms
{
    public class UpsertForm : Form
    {
        private TextBox txtTitle;
        private ComboBox cboSubject;
        private Button btnAddSubjectQuick; // Nút thêm môn nhanh
        private Label lblFileStatus;
        private Button btnBrowse;
        private TextBox txtNotes;
        private CheckBox chkStatus;

        private byte[] _fileBytes = null;
        private string _fileName = null;
        private string _fileExt = null;

        private Document _existingDoc;

        public UpsertForm(Document doc = null)
        {
            _existingDoc = doc;
            InitializeUI();
            LoadSubjects();

            if (doc != null)
            {
                this.Text = "Cập nhật tài liệu";
                txtTitle.Text = doc.Title;
                if (doc.SubjectId != null)
                {
                    cboSubject.SelectedValue = doc.SubjectId;
                }
                // --------------------

                txtNotes.Text = doc.Notes;
                chkStatus.Checked = doc.Status;

                // Báo cho người dùng biết đang dùng file cũ
                lblFileStatus.Text = string.IsNullOrEmpty(doc.FileName) ? "Đã có file cũ" : doc.FileName;
                lblFileStatus.ForeColor = Color.Blue;
            }
        }

        private void InitializeUI()
        {
            this.Text = "Thêm tài liệu mới";
            this.Size = new Size(550, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 20;
            this.Controls.Add(new Label { Text = "Tiêu đề tài liệu (*):", Location = new Point(20, y), AutoSize = true });
            txtTitle = new TextBox { Location = new Point(140, y - 3), Width = 300 };
            this.Controls.Add(txtTitle);

            y += 40;
            this.Controls.Add(new Label { Text = "Môn học:", Location = new Point(20, y), AutoSize = true });
            cboSubject = new ComboBox { Location = new Point(140, y - 3), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList }; // Thu nhỏ lại xíu

            // Nút thêm môn nhanh (+)
            btnAddSubjectQuick = new Button { Text = "+", Location = new Point(410, y - 4), Width = 30, Height = 23, BackColor = Color.Green, ForeColor = Color.White };

            this.Controls.AddRange(new Control[] { cboSubject, btnAddSubjectQuick });

            y += 40;
            this.Controls.Add(new Label { Text = "File đính kèm (*):", Location = new Point(20, y), AutoSize = true });
            btnBrowse = new Button { Text = "Chọn File...", Location = new Point(140, y - 5), Width = 100 };
            lblFileStatus = new Label { Text = "Chưa chọn file", Location = new Point(250, y), AutoSize = true, ForeColor = Color.Red };
            this.Controls.AddRange(new Control[] { btnBrowse, lblFileStatus });

            y += 40;
            this.Controls.Add(new Label { Text = "Ghi chú:", Location = new Point(20, y), AutoSize = true });
            txtNotes = new TextBox { Location = new Point(140, y - 3), Width = 300, Height = 100, Multiline = true };
            this.Controls.Add(txtNotes);

            y += 110;
            chkStatus = new CheckBox { Text = "Đã học xong", Location = new Point(140, y), AutoSize = true };
            this.Controls.Add(chkStatus);

            y += 40;
            var btnSave = new Button { Text = "Lưu lại", Location = new Point(140, y), Width = 100, Height = 35, BackColor = Color.Teal, ForeColor = Color.White };
            var btnCancel = new Button { Text = "Hủy", Location = new Point(250, y), Width = 100, Height = 35 };
            this.Controls.AddRange(new Control[] { btnSave, btnCancel });

            // Events
            btnBrowse.Click += BtnBrowse_Click;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.Close();

            // Sự kiện nút cộng thêm môn
            btnAddSubjectQuick.Click += (s, e) =>
            {
                using (var f = new SubjectForm())
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        LoadSubjects(); // Load lại danh sách ngay lập tức
                    }
                }
            };
        }

        private void LoadSubjects()
        {
            // Lưu lại ID môn đang chọn (nếu có) để sau khi reload thì select lại nó
            var selectedId = cboSubject.SelectedValue;

            cboSubject.DataSource = SubjectDAO.GetAll();
            cboSubject.DisplayMember = "Name";
            cboSubject.ValueMember = "SubjectId";

            if (selectedId != null) cboSubject.SelectedValue = selectedId;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var open = new OpenFileDialog())
            {
                open.Filter = "Tài liệu học tập|*.pdf;*.docx;*.doc;*.xlsx;*.pptx;*.txt";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var fileInfo = new FileInfo(open.FileName);
                        if (fileInfo.Length > 10 * 1024 * 1024)
                        {
                            MessageBox.Show("File quá lớn! Vui lòng chọn file dưới 10MB.");
                            return;
                        }

                        _fileBytes = File.ReadAllBytes(open.FileName);
                        _fileName = fileInfo.Name;
                        _fileExt = fileInfo.Extension.Replace(".", "");

                        lblFileStatus.Text = _fileName;
                        lblFileStatus.ForeColor = Color.Green;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi đọc file: " + ex.Message);
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Vui lòng nhập tiêu đề!");
                return;
            }

            // Nếu là thêm mới (Doc cũ null) thì BẮT BUỘC phải có file
            if (_existingDoc == null && _fileBytes == null)
            {
                MessageBox.Show("Vui lòng chọn một file tài liệu!");
                return;
            }

            try
            {
                var doc = new Document
                {
                    Title = txtTitle.Text.Trim(),
                    SubjectId = cboSubject.SelectedValue as int?,
                    Notes = txtNotes.Text,
                    Status = chkStatus.Checked,
                    UserId = Session.CurrentUser.UserId,

                    // Dữ liệu file (nếu _fileBytes null nghĩa là không chọn file mới)
                    FileData = _fileBytes,
                    FileName = _fileName,
                    Type = _fileExt
                };

                if (_existingDoc == null)
                {
                    // Thêm mới
                    DocumentDAO.Insert(doc);
                }
                else
                {
                    // Cập nhật (Gán ID cũ vào để SQL biết sửa dòng nào)
                    doc.DocumentId = _existingDoc.DocumentId;
                    DocumentDAO.Update(doc);
                }

                MessageBox.Show("Lưu thành công!");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu DB: " + ex.Message);
            }
        }
    }
}