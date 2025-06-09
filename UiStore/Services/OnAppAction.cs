using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UiStore.Services
{
    internal class OnAppAction<T, M>
    {
        private readonly List<RelayAction<T, M>>OnActions;
        private readonly M _model;
        public OnAppAction(M model)
        {
            OnActions = new List<RelayAction<T, M>>();
            _model = model;
        }

        public void Add(RelayAction<T, M> action)
        {
            if (action == null) return;
            OnActions.Add(action);
        }

        public void Remove(string name)
        {
            foreach (var action in OnActions)
            {
                if(action.Name == name)
                {
                    OnActions.Remove(action);
                }
            }
        }
        public void RunActions()
        {
            foreach (var action in OnActions)
            {
                if (action.CanAction == null || action.CanAction.Invoke(_value, _model))
                {
                    action.Action.Invoke(_value, _model);
                }
            }
        }

        internal void SetValue(T value)
        {
            _value = value;
            RunActions();
        }

        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                if (_value == null || !_value.Equals(value))
                {
                    SetValue(value);
                }
            }
        }
    }
}
