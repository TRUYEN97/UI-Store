using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using UiStore.Common;
using UiStore.Configs;
using UiStore.Models;
using UiStore.Services;
using UiStore.View;
using UiStore.ViewModels;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace UiStore.ViewModel
{
    internal class AppViewModel : BaseViewModel
    {
        private static readonly Brush RunningBrush = Brushes.LightGreen;
        private static readonly Brush UpdatingBrush = Brushes.LightYellow;
        private static readonly Brush StandbyBrush = Brushes.LightBlue;
        private static readonly Brush CreatingBrush = Brushes.LightSkyBlue;
        private readonly AppUnit _unit;

        public ICommand LaunchCommand { get; }
        public ICommand ShowInfoCommand { get; }

        public AppViewModel(CacheManager cache)
        {
            _unit = new AppUnit(cache);
            _backgroundColor = CreateSafeProperty(nameof(BackgroundColor),StandbyBrush);
            _iconSource = CreateSafeProperty<ImageSource>(nameof(IconSource));
            LaunchCommand = new RelayCommand( _ => _unit.LaunchApp());
            ShowInfoCommand = new RelayCommand( _ => ShowInfo());
        }

        public void Init(Action<string> logAction, Action<AppViewModel> addAppAction, Action<AppViewModel> removeAppAction)
        {
            _unit.AddAppAction = () =>
            {
                addAppAction?.Invoke(this);
            };
            _unit.RemoveAppAction = () =>
            {
                removeAppAction?.Invoke(this);
            };
            _unit.OnLog += logAction;
            _unit.OnProgress += p => Progress = p;
            _unit.OnIconFileChanged += ExtractIconFromApp;
            _unit.OnStatusChanged += OnStatusChanged;
            _unit.IsCanOpen += IsCanOpen;
            _unit.Init();
        }
        public void StartUpdate() => _unit.StartUpdate();
        public void StopUpdate() => _unit.StopUpdate();
        public bool Isrunning => _unit.IsRunning;

        public string ProgramFolderPath
        {
            get => SafeGet(() => _unit.ProgramFolderPath);
            set => SafeSet(() => _unit.ProgramFolderPath = value);
        }
        
        public string CommonFolderPath
        {
            get => SafeGet(() => _unit.CommonFolderPath);
            set => SafeSet(() => _unit.CommonFolderPath = value);
        }

        public string Name
        {
            get => SafeGet(() => _unit.Name);
            set => SafeSet(() => _unit.Name = value);
        }

        public string AppModelPath
        {
            get => SafeGet(() => _unit.AppModelPath);
            set => SafeSet(() => _unit.AppModelPath = value);
        }

        public string Version => _unit.Version;

        public string LocalPath => _unit.LocalPath;



        private readonly SafeDispatcherProperty<ImageSource> _iconSource;
        public ImageSource IconSource
        {
            get => _iconSource.Value;
            set => _iconSource.Value = value;
        }

        private readonly SafeDispatcherProperty<Brush> _backgroundColor;

        public Brush BackgroundColor
        {
            get => _backgroundColor.Value;
            set => _backgroundColor.Value = value;
        }

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public bool IsHovered
        {
            get => _isHovered;
            set => SetProperty(ref _isHovered, value);
        }

        private bool _isHovered;

        private void ShowInfo()
        {
            var window = new AppInfoWindow(this)
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        private void OnStatusChanged()
        {
            if (_unit.IsRunning)
            {
                BackgroundColor = RunningBrush;
            }
            else
            {
                switch (_unit.Status)
                {
                    case 0:
                        BackgroundColor = StandbyBrush;
                        break;
                    case 1:
                        BackgroundColor = UpdatingBrush;
                        break;
                    case 2:
                        BackgroundColor = CreatingBrush;
                        break;
                    default:
                        BackgroundColor = StandbyBrush;
                        break;
                }
            }
        }

        private bool IsCanOpen(Dictionary<string, string> accs)
        {
            return LoginWindow.IsPassword(accs);
        }

        private void ExtractIconFromApp(string path)
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
    }

}
