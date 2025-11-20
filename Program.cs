using System;
using System.Windows.Forms;
using StudyDocs.Forms;
using StudyDocs.Services; // Thêm cái này

namespace StudyDocs
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoginForm login = new LoginForm();
            if (login.ShowDialog() == DialogResult.OK)
            {
                // BỎ DÒNG MessageBox test đi
                // MỞ DÒNG NÀY RA:
                Application.Run(new MainForm());
            }
            else
            {
                Application.Exit();
            }
        }
    }
}