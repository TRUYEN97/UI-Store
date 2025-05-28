using System;
using System.Collections.Concurrent;
using System.IO;
using UiStore.Common;

namespace UiStore.Services
{
    internal class CacheManager
    {
        private readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

        public void LoadFromFolder(string baseFolder)
        {
            if(Directory.Exists(baseFolder))
            {
                foreach (var file in Directory.EnumerateFiles(baseFolder, "*.*", SearchOption.AllDirectories))
                {
                    string md5 = Path.GetFileNameWithoutExtension(file);
                    _cache[md5] = file;
                }
            }
        }

        public bool TryGetPathByMd5(string md5, out string path)
        {
            if (_cache.TryGetValue(md5, out path))
            {
                if(File.Exists(path))
                {
                    return true;
                }
                _cache.TryRemove(md5, out _);
            }
            path = null;
            return false;
        }

        public void Add(string md5, string path)
        {
            _cache[md5] = path;
        }

        internal bool TryRemove(string md5)
        {
            return _cache.TryRemove(md5, out _);
        }
    }
}
