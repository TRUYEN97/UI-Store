using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UiStore.Common;

namespace UiStore.View
{
    /// <summary>
    /// Interaction logic for PasswordWindow.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        private readonly string CorrectPassword;

        private PasswordWindow(string password)
        {
            InitializeComponent();
            CorrectPassword = password;
            this.PasswordBox.Focus();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            CheckPassword();
        }

        private void CheckPassword()
        {
            string pwMd5 = Util.GetMD5HashFromString(PasswordBox.Password.Trim());
            if (pwMd5 == null || pwMd5 == CorrectPassword)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Sai mật khẩu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Clear();
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckPassword();
            }
        }

        public static bool IsPassword(string password)
        {
            return new PasswordWindow(password).ShowDialog() == true;
        }
    }

}
