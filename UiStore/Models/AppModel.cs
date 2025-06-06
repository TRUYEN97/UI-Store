using System.Collections.Generic;

namespace UiStore.Models
{
    internal class AppModel
    {
        public string OpenCmd { get; set; }
        public string CloseCmd { get; set; }
        public string MainPath { get; set; }
        public string Version { get; set; }
        public string FWSersion { get; set; }
        public string FCDVersion { get; set; }
        public string BOMVersion { get; set; }
        public string FTUVersion { get; set; }
        public string RemoteStoreDir { get; set; }
        public string RemoteAppListPath { get; set; }
        public string AppIconPath { get; set; }
        public FileModel AppIconFileModel { get; set; }
        public string Path { get; set; }
        public bool Enable { get; set; }
        public bool AutoOpen { get; set; }
        public bool AutoUpdate { get; set; }
        public bool AutoRemove { get; set; }
        public bool CloseAndClear { get; set; }

        public HashSet<FileModel> FileModels { get; set; } = new HashSet<FileModel>();
    }
}
