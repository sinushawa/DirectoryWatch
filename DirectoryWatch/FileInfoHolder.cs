using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DirectoryWatch
{
    [Serializable]
    public class FileInfoHolder
    {
        public string Name;
        public string Extension;
        public DateTime LastWriteTime;

        public FileInfoHolder(FileInfo _fi)
        {
            Name = _fi.Name;
            Extension = _fi.Extension;
            LastWriteTime = _fi.LastWriteTime;
        }
    }
    public static class FileExtension
    {
        public static FileInfoHolder ToFileInfoHolder(this FileInfo _fi)
        {
            return new FileInfoHolder(_fi);
        }
    }
}
