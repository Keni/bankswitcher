using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace BankSwitcher
{
    public partial class LoginForm : Form
    {
        string adminPass = "";
        string userPass = "";
        public static bool test = false;
        public LoginForm()
        {
            InitializeComponent();

            MainForm.logToFile("Приложение было запущено");

            this.AcceptButton = buttonLogin;                        
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string password = generateSHA256(textBoxPassword.Text);

            if (password.Equals(userPass) || password.Equals(adminPass))
            {
                MainForm.logToFile("Пароль введен верно");
                test = true;

                MainForm mainForm = new MainForm();
                this.Hide();
                mainForm.ShowDialog();
                this.Close();
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
            Console.WriteLine(hash);
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

    }
}
