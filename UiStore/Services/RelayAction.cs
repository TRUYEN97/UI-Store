using System;

namespace UiStore.Services
{
    internal class RelayAction<T, M>
    {
        private readonly Func<T, M, bool> _canAction;
        private readonly Action<T, M> _action;
        public RelayAction(Action<T, M> action) : this(null, null, action) { }
        public RelayAction(Func<T, M, bool> canAction, Action<T, M> action) : this(null, canAction, action) { }
        public RelayAction(string name, Action<T, M> action) : this(name, null, action) { }

        public RelayAction(string name, Func<T, M, bool> canAction, Action<T, M> action)
        {
            Name = name ?? "";
            _canAction = canAction;
            _action = action;
        }

        public string Name { get; private set; }

        public Func<T, M, bool> CanAction => _canAction;
        public Action<T, M> Action => _action;
    }
}
