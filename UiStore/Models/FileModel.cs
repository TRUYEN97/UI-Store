using System;

namespace UiStore.Models
{
    internal class FileModel
    {
        public string ProgramPath { get; set; }
        public string RemotePath { get; set; }
        public string Md5 { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is FileModel other)
            {
                return string.Equals(this.ProgramPath, other.ProgramPath, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.ProgramPath?.ToLower().GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return this.ProgramPath;
        }
    }
}
