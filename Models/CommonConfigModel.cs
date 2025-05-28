using System;
using System.Collections.Generic;

namespace UiStore.Model
{
    internal class CommonConfigModel
    {
        public HashSet<FileModel> FileModels { get; set; } = new HashSet<FileModel>();
        public string RemoteDir { get; set; } = "";
    }
}
