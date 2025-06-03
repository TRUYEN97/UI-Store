
using System.Linq;
using System.Windows;
using System.Windows.Input;
using UiStore.Services;
using UiStore.View;
using UiStore.Common;
using System.Collections.Generic;
using System.IO;
using System;
using UiStore.Models;
using Microsoft.Extensions.Logging;

namespace UiStore.ViewModels
{
    internal class LoginViewModel : BaseViewModel
    {
        private readonly Logger logger;
        public HashSet<UserModel> UserModels {  get; set; }
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

        public LoginViewModel(HashSet<UserModel> users, Logger logger) : this(logger)
        {
            this.UserModels = users;
            Id = null;
            Password = null;
        }

        public LoginViewModel(Logger logger)
        {
            this.logger = logger;
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private bool IsAccount(HashSet<UserModel> accountModel)
        {
            if (accountModel == null)
            {
                return true;
            }
            foreach (var userModel in accountModel)
            {
                if (userModel?.Id == this.Id)
                {
                    string pwMd5 = Util.GetMD5HashFromString(this.Password?.Trim());
                    return pwMd5 == userModel?.Password;
                }
            }
            return false;
        }

        private void ExecuteLogin(object obj)
        {
            if (UserModels == null || UserModels.Count == 0)
            {
                var wd = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w is LoginWindow);
                if (wd != null)
                {
                    wd.DialogResult = true;
                    return;
                }
            }
            else
            {
                if (IsAccount(UserModels))
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
            this.logger?.AddLogLine($"User [{Id}] login");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}]  login: [{Id}]\r\n");
        }
    }
}
