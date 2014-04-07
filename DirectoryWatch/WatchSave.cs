using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryWatch
{
    [Serializable]
    public class WatchSave
    {
        public string watchedFolder;
        public SortableObservableCollection<FileTypeFilter> filters;
        public WatchSave()
        {
        }
        public WatchSave(string _watchedFolder, SortableObservableCollection<FileTypeFilter> _filters)
        {
            watchedFolder = _watchedFolder;
            filters = _filters;
        }
    }
}
