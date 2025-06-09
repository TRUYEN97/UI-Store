using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UiStore.Common;
using UiStore.Models;
using UiStore.Services;
using UiStore.View;
using UiStore.ViewModels;
using static UiStore.Services.AppStatusInfo;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace UiStore.ViewModel
{
    internal class AppViewModel : BaseViewModel
    {
        private static readonly Brush RunningBrush = Brushes.LightGreen;
        private static readonly Brush UpdatingBrush = Brushes.Yellow;
        private static readonly Brush StandbyBrush = Brushes.LightBlue;
        private static readonly Brush HasNewVersionBrush = Brushes.Orange;
        private static readonly Brush DeletedBrush = Brushes.Black;
        private static readonly Brush UpdateFailedBrush = Brushes.Red;
        private static readonly Brush TransparentBrush = Brushes.Transparent;
        private readonly AppUnit _appUnit;
        public ICommand LaunchCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ShowInfoCommand { get; }

        public AppViewModel(CacheManager cache, ProgramManagement programManagement, AppInfoModel appInfoModel, Logger logger)
        {
            Name = appInfoModel.Name;
            _appUnit = new AppUnit(cache, programManagement, appInfoModel, this, logger);
            _backgroundColor = CreateSafeProperty(nameof(BackgroundColor), StandbyBrush);
            _statusBackgroundColor = CreateSafeProperty(nameof(StatusBackgroundColor), StandbyBrush);
            _runningBackgroundColor = CreateSafeProperty(nameof(RunningBackgroundColor), StandbyBrush);
            _iconSource = CreateSafeProperty<ImageSource>(nameof(IconSource));
            LaunchCommand = new RelayCommand(_ => _appUnit.LaunchApp());
            CloseCommand = new RelayCommand(_ => _appUnit.CloseApp());
            ShowInfoCommand = new RelayCommand(_ => ShowInfo());
        }

        public void StartUpdate() => _appUnit.StartUpdate();
        public void StopUpdate() => _appUnit.StopUpdate();
        public string Name { get => name; set { name = value; OnPropertyChanged(); } }
        public string FWVersion { get => fWVersion; set { fWVersion = value; OnPropertyChanged(); } }
        public string FCDVersion { get => fCDVersion; set { fCDVersion = value; OnPropertyChanged(); } }
        public string BOMVersion { get => bOMVersion; set { bOMVersion = value; OnPropertyChanged(); } }
        public string FTUVersion { get => fTUVersion; set { fTUVersion = value; OnPropertyChanged(); } }
        public string Version { get => version; set { version = value; OnPropertyChanged(); } }

        public AppInfoModel AppInfoModel
        {
            get => SafeGet(() => _appUnit.InstanceWarehouse.AppInfoModel);
        }

        private readonly SafeDispatcherProperty<ImageSource> _iconSource;
        public ImageSource IconSource
        {
            get => _iconSource.Value;
            set => _iconSource.Value = value;
        }

        private readonly SafeDispatcherProperty<Brush> _backgroundColor;
        private readonly SafeDispatcherProperty<Brush> _statusBackgroundColor;
        private readonly SafeDispatcherProperty<Brush> _runningBackgroundColor;

        public Brush BackgroundColor
        {
            get => _backgroundColor.Value;
            set => _backgroundColor.Value = value;
        }
        public Brush StatusBackgroundColor
        {
            get => _statusBackgroundColor.Value;
            set => _statusBackgroundColor.Value = value;
        }
        public Brush RunningBackgroundColor
        {
            get => _runningBackgroundColor.Value;
            set => _runningBackgroundColor.Value = value;
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }


        private bool _isHovered;
        private string name;
        private string fWVersion;
        private string fCDVersion;
        private string bOMVersion;
        private string fTUVersion;
        private string version;
        private bool _isNewVersion;
        private bool _isDeleted;

        public bool IsHovered
        {
            get => _isHovered;
            set => SetProperty(ref _isHovered, value);
        }

        private void ShowInfo()
        {
            var window = new AppInfoWindow(this)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        internal void ExtractIconFromApp(string path)
        {
            try
            {
                Icon icon;
                if (path == null || !File.Exists(path))
                {
                    icon = SystemIcons.Application;
                }
                else
                {
                    icon = Icon.ExtractAssociatedIcon(path) ?? SystemIcons.Application;
                }
                using (icon)
                {
                    DispatcherHelper.RunOnUI(() =>
                    {
                        IconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromWidthAndHeight(128, 128));
                    });
                }
            }
            catch
            {
                return;
            }
        }

        internal void SetRunning(bool isRunning)
        {
            DispatcherHelper.RunOnUI(() =>
            {
                if (isRunning)
                    RunningBackgroundColor = RunningBrush;
                else
                    RunningBackgroundColor = StandbyBrush;
            });
        }

        private void OnStatusChanged()
        {
            DispatcherHelper.RunOnUI(() =>
            {
                if (_isDeleted)
                    StatusBackgroundColor = DeletedBrush;
                else if (_isNewVersion)
                    StatusBackgroundColor = HasNewVersionBrush;
                else
                    StatusBackgroundColor = TransparentBrush;
            });
        }

        internal void SetNewVersionStatus(bool isNewVersion)
        {
            _isNewVersion = isNewVersion;
            OnStatusChanged();
        }

        internal void SetDeletedStatus(bool isDeleted)
        {
            _isDeleted = isDeleted;
            OnStatusChanged();
        }

        internal void SetUpdateStatus(UpdateState status)
        {
            DispatcherHelper.RunOnUI(() =>
            {
                switch (status)
                {
                    case UpdateState.SUCCESS:
                        BackgroundColor = StandbyBrush;
                        break;
                    case UpdateState.FAILED:
                        BackgroundColor = UpdateFailedBrush;
                        break;
                    case UpdateState.UPDATING:
                        BackgroundColor = UpdatingBrush;
                        break;
                    default:
                        BackgroundColor = StandbyBrush;
                        break;
                }
            });
        }
    }

}
