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
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        private string fullName;
        public string FullName
        {
            get
            {
                return fullName;
            }
            set
            {
                fullName = value;
            }
        }
        private string extension;
        public string Extension
        {
            get
            {
                return extension;
            }
            set
            {
                extension = value;
            }
        }
        private DateTime lastWriteTime;
        public DateTime LastWriteTime
        {
            get
            {
                return lastWriteTime;
            }
            set
            {
                lastWriteTime = value;
            }
        }

        public FileInfoHolder(FileInfo _fi)
        {
            FullName = _fi.FullName;
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
