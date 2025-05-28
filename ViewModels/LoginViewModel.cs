
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UiStore.Services;
using UiStore.View;
using UiStore.Common;
using System.Collections.Generic;
using System.IO;
using System;

namespace UiStore.ViewModels
{
    internal class LoginViewModel : BaseViewModel
    {
        private readonly Dictionary<string, string> accs;
        private static readonly string logPath = "loginLog.txt";

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(Dictionary<string, string> accs)
        {
            this.accs = accs;
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private bool IsAccount(Dictionary<string, string> accountModel)
        {
            if (accountModel == null)
            {
                return true;
            }
            if (accountModel.TryGetValue(this.Id, out var passsword))
            {
                string pwMd5 = Util.GetMD5HashFromString(Password?.Trim());
                return pwMd5 == passsword;
            }
            return false;
        }

        private void ExecuteLogin(object obj)
        {
            if (accs == null || accs.Count == 0)
            {
                return;
            }
            if (IsAccount(accs))
            {
                var wd = Application.Current.Windows
               .OfType<Window>()
               .FirstOrDefault(w => w is LoginWindow);
                if (wd != null)
                {
                    AddLoginLog();
                    wd.DialogResult = true;
                    return;
                }
            }
            MessageBox.Show("Sai mật khẩu!");
        }

        private void AddLoginLog()
        {
            if (!File.Exists(logPath))
            {
                if (Directory.Exists(Path.GetDirectoryName(logPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                }
            }
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}]  login: {Id}\r\n");
        }
    }
}
