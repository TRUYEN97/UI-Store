using System;
using System.Collections.Generic;

namespace UiStore.Services
{
    internal class OnAppAction<T, M>
    {
        private readonly List<RelayAction<T, M>> _onActions;
        private readonly Queue<RelayAction<T, M>> _oneTimeAction;
        private readonly M _model;
        public OnAppAction(M model)
        {
            _onActions = new List<RelayAction<T, M>>();
            _oneTimeAction = new Queue<RelayAction<T, M>>();
            _model = model;
        }

        public void Add(RelayAction<T, M> action)
        {
            if (action == null) return;
            _onActions.Add(action);
        }
        
        public void AddOneTimeAction(RelayAction<T, M> action)
        {
            if (action == null) return;
            _oneTimeAction.Enqueue(action);
        }
        
        public void ClearQueue()
        {
            _oneTimeAction.Clear();
        }
        public void ClearAction()
        {
            _onActions.Clear();
        }

        public void Remove(string name)
        {
            foreach (var action in _onActions)
            {
                if(action.Name == name)
                {
                    _onActions.Remove(action);
                }
            }
        }

        public void RunActions()
        {
            foreach (var action in _onActions)
            {
                if (action.CanAction == null || action.CanAction.Invoke(_value, _model))
                {
                    action.Action.Invoke(_value, _model);
                }
            }
        }
        
        public void RunOnetimeActions()
        {
            for (int i = 0; i < _oneTimeAction.Count; i++)
            {
                var action = _oneTimeAction.Peek();
                if (action?.CanAction == null || action.CanAction?.Invoke(_value, _model) == true)
                {
                    _oneTimeAction.Dequeue()?.Action?.Invoke(_value,_model);
                }
            }
        }

        internal void SetValue(T value)
        {
            _value = value;
            RunActions();
            RunOnetimeActions();
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
