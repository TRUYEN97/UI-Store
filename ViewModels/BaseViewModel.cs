
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using UiStore.Common;

namespace UiStore.ViewModels
{
    internal abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected BaseViewModel() { }
        public event PropertyChangedEventHandler PropertyChanged;
        protected SafeDispatcherProperty<T> CreateSafeProperty<T>(string name, T initialValue = null) where T : class
        {
            return new SafeDispatcherProperty<T>(
                Application.Current.Dispatcher,
                propertyName: name,
                raisePropertyChanged: (prop) => OnPropertyChanged(prop),
                initialValue: initialValue
            );
        }

        private string GetPropertyName([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) => propertyName;


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == true)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            else
                Application.Current?.Dispatcher?.Invoke(() =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name))
                );
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(name);
            return true;
        }

        protected T SafeGet<T>(Func<T> getter)
        {
            return DispatcherHelper.CheckAccess()
                ? getter()
                : DispatcherHelper.RunOnUI(getter);
        }

        protected void SafeSet(Action setter)
        {
            if (DispatcherHelper.CheckAccess())
                setter();
            else
                DispatcherHelper.RunOnUI(setter);
        }
    }
}
