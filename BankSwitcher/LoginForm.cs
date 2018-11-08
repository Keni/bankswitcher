using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace BankSwitcher
{
    public partial class LoginForm : Form
    {
        string adminPass = "password";
        string userPass = "password";
        string hrPass = "password";
        public static bool test = false;
        public static bool hr = false;
        public LoginForm()
        {
            Process[] processes = Process.GetProcessesByName(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            if (processes.Length <= 1)
            {
                InitializeComponent();

                MainForm.logToFile("Приложение было запущено");

                this.AcceptButton = buttonLogin;
            }
            else
            {
                MessageBox.Show("Приложение уже запущенно", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string password = generateSHA256(textBoxPassword.Text);

            if (password.Equals(userPass) || password.Equals(adminPass) || password.Equals(hrPass))
            {
                MainForm.logToFile("Пароль введен верно");
                test = true;

                if (password.Equals(hrPass))
                {
                    hr = true;
                }

                entry();
            }
            else
            {
                MainForm.logToFile("Был введен неверный пароль");
                MessageBox.Show("Неверный пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string generateSHA256(string password)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);
            return getStringFromHash(hash);
        }

        private string getStringFromHash(byte[] hash)
        {
            StringBuilder password = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                password.Append(hash[i].ToString("X2"));
            }

            return password.ToString();
        }

        private void entry()
        {
            MainForm mainForm = new MainForm();
            this.Hide();
            mainForm.ShowDialog();
            this.Close();
        }

    }
}
