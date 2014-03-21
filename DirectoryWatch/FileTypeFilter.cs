using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryWatch
{
    public class FileTypeFilter
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
                name = (string)value;
            }
        }
        private bool isEnabled;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = (bool)value;
            }
        }
        private List<string> extensions;
        public List<string> Extensions
        {
            get
            {
                return extensions;
            }
            set
            {
                extensions = (List<string>)value;
            }
        }
        public FileTypeFilter(string _name, bool _isEnabled, List<string> _extensions)
        {
            name = _name;
            isEnabled = _isEnabled;
            extensions = _extensions;
        }
    }
}
