using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiStore.Services
{
    internal class OnAppAction<T>
    {
        private readonly Dictionary<string, Action<T>> OnActions;
        public OnAppAction()
        {
            OnActions = new Dictionary<string, Action<T>>();
        }

        public void AddAction(string name, Action<T> action)
        {
            if (string.IsNullOrEmpty(name) || action == null) return;
            OnActions[name] = action;
        }
        public void RemoveAction(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            OnActions.Remove(name);
        }

        public void OnActiveChanged()
        {
            foreach (var action in OnActions.Values)
            {
                action?.Invoke(_value);
            }
        }

        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (_value == null || !_value.Equals(value))
                {
                    _value = value;
                    OnActiveChanged();
                }
            }
        }
    }
}
