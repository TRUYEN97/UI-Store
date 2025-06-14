using System;

namespace UiStore.Common
{
    internal class ConnectFaildedException: Exception
    {
        public ConnectFaildedException(string msg): base(msg) { }
    }
}
