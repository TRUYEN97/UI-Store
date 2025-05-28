using System;
using System.Collections.Generic;

namespace UiStore.Model
{
    internal class AppModel
    {
        public string OpenCmd { get; set; }
        public string CloseCmd { get; set; }
        public string MainPath { get; set; }
        public string WindowTitle { get; set; }
        public HashSet<FileModel> FileModels { get; set; } = new HashSet<FileModel>();
        public bool Enable { get; set; }
        public bool AutoOpen { get; set; }
        public string LocalPath { get; internal set; }
        public string Version { get; internal set; }
    }
}
