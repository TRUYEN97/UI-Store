using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using UiStore.Common;
using UiStore.Configs;
using UiStore.Services;
using UiStore.View;
using UiStore.ViewModel;

namespace UiStore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        private bool _isExit;
        public MainWindow()
        {
            Util.RunCmd($"attrib +h +s \"{AutoDLConfig.ConfigModel.AppLocalPath}\"");
            Util.RunCmd($"attrib +h +s \"{AutoDLConfig.ConfigModel.CommonLocalPath}\"");
            Util.RunCmd($"attrib +h +s \"{AutoDLConfig.CfPath}\"");
            if (PasswordWindow.IsPassword(AutoDLConfig.ConfigModel.LaunchPassword))
            {
                InitializeComponent();
                InitMainViewMode();
                InitTimer();
            }
            else
            {
                ExitApplication();
                return;
            }
        }

        private void InitMainViewMode()
        {
            var cache = new CacheManager();
            cache.LoadFromFolder(AutoDLConfig.ConfigModel.CommonLocalPath);
            var _viewModel = new MainViewModel(cache);
            DataContext = _viewModel;
            Loaded += (_, o) =>
            {
                _viewModel.Start();
            };
        }

        private void InitTimer()
        {
            var checkShowSignal = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            checkShowSignal.Tick += (s, e) =>
            {
                if (File.Exists("show.signal"))
                {
                    File.Delete("show.signal");
                    ShowWindow();
                }
                if (File.Exists("shutdown.signal"))
                {
                    File.Delete("shutdown.signal");
                    ExitApplication();
                }
            };
            checkShowSignal.Start();
        }

        private void CreateNotifyIcon()
        {
            if (_notifyIcon != null) 
            {
                return;
            }
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "Ui Store";

            _notifyIcon.DoubleClick += (s, args) => ShowWindow();

            // Menu chuột phải
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Mở lại").Click += (s, e) => ShowWindow();
            contextMenu.Items.Add("Thoát").Click += (s, e) => ExitApplication();
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void ExitApplication()
        {
            _isExit = true;
            _notifyIcon?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExit)
            {
                CreateNotifyIcon();
                e.Cancel = true;
                this.Hide();
            }
            base.OnClosing(e);
        }

    }
}
