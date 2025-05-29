using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace UiStore.Common
{
    public class SafeDispatcherProperty<T>
    {
        private T _value;
        private readonly Dispatcher _dispatcher;
        private readonly Action<string> _raisePropertyChanged;
        private readonly string _propertyName;

        public SafeDispatcherProperty(Dispatcher dispatcher = null, Action<string> raisePropertyChanged = null, string propertyName = null, T initialValue = default)
        {
            _dispatcher = dispatcher ?? Application.Current.Dispatcher;
            _raisePropertyChanged = raisePropertyChanged;
            _propertyName = propertyName;
            _value = initialValue;
        }

        public T Value
        {
            get
            {
                if (_dispatcher.CheckAccess())
                    return _value;
                else
                    return _dispatcher.Invoke(() => _value);
            }
            set
            {
                if (_dispatcher.CheckAccess())
                {
                    if (!Equals(_value, value))
                    {
                        _value = value;
                        _raisePropertyChanged?.Invoke(_propertyName);
                    }
                }
                else
                {
                    _dispatcher.Invoke(() => Value = value);
                }
            }
        }
    }


}
