using System;
using System.Threading.Tasks;
using UiStore.Common;
using UiStore.Models;
using UiStore.View;
using UiStore.ViewModels;

namespace UiStore.Services
{
    internal class Authentication
    {
        private readonly Logger logger;


        public Authentication(Logger logger, string accessUserListModelPath) : this(logger)
        {
            this.AccessUserListModelPath = accessUserListModelPath;
        }

        public Authentication(Logger logger)
        {
            this.logger = logger;
        }

        public string _accessUserListModelPath;
        public string AccessUserListModelPath
        {
            get => _accessUserListModelPath;
            set
            {
                if (string.IsNullOrEmpty(value) || !value.Equals(_accessUserListModelPath, StringComparison.OrdinalIgnoreCase))
                {
                    _accessUserListModelPath = value;
                    IsAuthenticated = false;
                }
            }
        }

        public bool IsAuthenticated { get; private set; }
        private LoginWindow LoginWindow;
        public async Task<bool> Login()
        {
            return await Login(false);
        }

        public async Task<bool> Login(bool remember)
        {
            bool rs = false;
            if (!string.IsNullOrEmpty(AccessUserListModelPath))
            {
                AccessUserListModel accessUserListModel = await TranforUtil.GetModelConfig<AccessUserListModel>(AccessUserListModelPath, ConstKey.ZIP_PASSWORD);
                if (accessUserListModel?.UserModels == null || accessUserListModel?.UserModels?.Count == 0)
                {
                    if (LoginWindow != null && LoginWindow.IsVisible)
                    {
                        DispatcherHelper.RunOnUI(() =>
                        {
                            LoginWindow.DialogResult = false;
                        });
                    }
                    rs = true;
                }
                else if (LoginWindow == null)
                {
                    DispatcherHelper.RunOnUI(() =>
                    {
                        LoginWindow = new LoginWindow(new LoginViewModel(accessUserListModel?.UserModels, logger));
                        rs = LoginWindow.ShowDialog() == true;
                        LoginWindow = null;
                    });
                }
            }
            if (remember)
            {
                IsAuthenticated = rs;
            }
            return rs;
        }
    }
}
