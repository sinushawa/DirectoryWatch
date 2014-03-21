using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DirectoryWatch
{
    public static class DirectoryInfoExtensions
    {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dirInfo, params string[] extensions)
        {
            var allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
            return dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Where(f => allowedExtensions.Contains(f.Extension));
        }
    }
}
