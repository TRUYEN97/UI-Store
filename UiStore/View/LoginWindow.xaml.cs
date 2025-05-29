using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UiStore.Common;
using UiStore.Models;
using UiStore.ViewModels;

namespace UiStore.View
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private LoginWindow(Dictionary<string, string> accs)
        {
            InitializeComponent();
            DataContext = new LoginViewModel(accs);
            IdBox.Focus();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }

        private void IdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        internal static bool IsPassword(Dictionary<string, string> accs)
        {
            if(new LoginWindow(accs).ShowDialog() == true)
            {
                return true;
            }
            return false;
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btOk.Focus();
            }
        }
    }

}
