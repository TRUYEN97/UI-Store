using System;

namespace UiStore.Common
{
    internal class SftpFileNotFoundException:Exception
    {
        public SftpFileNotFoundException(string message) : base(message) { }
    }
}
