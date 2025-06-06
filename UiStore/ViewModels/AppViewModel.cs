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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static UiStore.Services.AppStatusInfo;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace UiStore.ViewModel
{
    internal class AppViewModel : BaseViewModel, IDisposable
    {
        private static readonly Brush RunningBrush = Brushes.LightGreen;
        private static readonly Brush UpdatingBrush = Brushes.Yellow;
        private static readonly Brush StandbyBrush = Brushes.LightBlue;
        private static readonly Brush CreatingBrush = Brushes.LightSkyBlue;
        private static readonly Brush HasNewVersionBrush = Brushes.Orange;
        private static readonly Brush DeletedBrush = Brushes.Black;
        private static readonly Brush UpdateFailedBrush = Brushes.Red;
        private static readonly Brush TransparentBrush = Brushes.Transparent;
        private readonly AppUnit _appUnit;
        private readonly AppEvent _appStatus;
        public ICommand LaunchCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ShowInfoCommand { get; }

        public AppViewModel(CacheManager cache, ProgramManagement programManagement, ProgramPathModel programPathModel, Logger logger)
        {
            _appUnit = new AppUnit(cache, programManagement, programPathModel, this, logger);
            _appStatus = _appUnit.AppEvent;
            _backgroundColor = CreateSafeProperty(nameof(StatusBackgroundColor), StandbyBrush);
            _statusBackgroundColor = CreateSafeProperty(nameof(StatusBackgroundColor), StandbyBrush);
            _runningBackgroundColor = CreateSafeProperty(nameof(RunningBackgroundColor), StandbyBrush);
            _iconSource = CreateSafeProperty<ImageSource>(nameof(IconSource));
            LaunchCommand = new RelayCommand(_ => _appUnit.LaunchApp());
            CloseCommand = new RelayCommand(_ => _appUnit.CloseApp());
            ShowInfoCommand = new RelayCommand(_ => ShowInfo());
            InitAppStatusAction();
        }

        public void StartUpdate() => _appUnit.StartUpdate();
        public void StopUpdate() => _appUnit.StopUpdate();

        public string Name => _appUnit?.AppInfoModel?.Name;
        public string FWVersion => _appUnit?.AppModel?.FWSersion;
        public string FCDVersion => _appUnit?.AppModel?.FCDVersion;
        public string BOMVersion => _appUnit?.AppModel?.BOMVersion;
        public string FTUVersion => _appUnit?.AppModel?.FTUVersion;
        public string Version => _appUnit?.AppModel?.Version;

        public AppInfoModel AppInfoModel
        {
            get => SafeGet(() => _appUnit.AppInfoModel);
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

        private void InitAppStatusAction()
        {
            _appStatus.RunningStatus.AddAction("Running", (b) =>
            {
                DispatcherHelper.RunOnUI(() =>
                {
                    if (b.Item1) RunningBackgroundColor = RunningBrush;
                    else RunningBackgroundColor = StandbyBrush;
                });
            });
            _appStatus.NewVersionStatus.AddAction("NewVersion", (b) =>
            {
                DispatcherHelper.RunOnUI(() =>
                {
                    if (b) StatusBackgroundColor = HasNewVersionBrush;
                    else StatusBackgroundColor = TransparentBrush;
                });
            });
            _appStatus.NewVersionStatus.AddAction("InfoChanged", (b) =>
            {
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(FWVersion));
                OnPropertyChanged(nameof(FCDVersion));
                OnPropertyChanged(nameof(BOMVersion));
                OnPropertyChanged(nameof(FTUVersion));
                OnPropertyChanged(nameof(Version));
            });
            _appStatus.UpdateAction.AddAction("update", (status) =>
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
            });
        }
        public void Dispose()
        {
            _appUnit.Dispose();
        }
    }

}
