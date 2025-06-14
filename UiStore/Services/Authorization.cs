using System;
using System.Threading.Tasks;
using UiStore.Common;
using UiStore.Models;
using UiStore.View;
using UiStore.ViewModels;

namespace UiStore.Services
{
    internal class Authorization
    {
        private readonly Logger _logger;
        private AccessUserListModel accessUserListModel;


        public Authorization(Logger logger, string accessUserListModelPath) : this(logger)
        {
            this.AccessUserListModelPath = accessUserListModelPath;
        }

        public Authorization(Logger logger)
        {
            this._logger = logger;
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
                }
            }
        }

        public bool IsConnected => TranforUtil.IsConnected();

        private LoginWindow LoginWindow;

        public async Task<bool> Login()
        {

            bool rs = false;
            if (!string.IsNullOrEmpty(AccessUserListModelPath) && await UpdateAccessUserListModel() && accessUserListModel?.UserModels != null)
            {
                if (accessUserListModel.UserModels?.Count == 0)
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
                        LoginWindow = new LoginWindow(new LoginViewModel(accessUserListModel.UserModels, _logger));
                        rs = LoginWindow.ShowDialog() == true;
                        LoginWindow = null;
                    });
                }
            }
            return rs;
        }

        private async Task<bool> UpdateAccessUserListModel()
        {
            try
            {
                AccessUserListModel accessUserListModel = await TranforUtil.GetModelConfig<AccessUserListModel>(AccessUserListModelPath, ConstKey.ZIP_PASSWORD);
                if (accessUserListModel?.UserModels == null)
                {
                    return false;
                }
                this.accessUserListModel = accessUserListModel;
                return true;
            }
            catch (ConnectFaildedException ex)
            {
                _logger.AddLogLine(ex.Message);
            }
            catch (SftpFileNotFoundException ex)
            {
                _logger.AddLogLine(ex.Message);
                if (accessUserListModel?.UserModels == null) accessUserListModel = new AccessUserListModel();
            }
            return this.accessUserListModel?.UserModels != null;
        } 
    }
}
