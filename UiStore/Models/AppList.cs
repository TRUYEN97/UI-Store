using System;
using System.Collections.Generic;

namespace UiStore.Models
{
    internal class AppList
    {
        public AppList() { 

        }
        public Dictionary<string, ProgramPathModel> ProgramPaths { get; set; } = new Dictionary<string, ProgramPathModel>();

    }
}
