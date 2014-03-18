using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

namespace DirectoryWatch
{
    partial class NotifyWatch : ResourceDictionary, INotifyPropertyChanged
    {
        # region FileNotifications dependency definition
        private static SortableObservableCollection<FileInfo> fileNotificationsProperty = new SortableObservableCollection<FileInfo>();
        public SortableObservableCollection<FileInfo> FileNotifications
        {
            get
            {
                return (SortableObservableCollection<FileInfo>)fileNotificationsProperty;
            }
            set
            {
                fileNotificationsProperty=value;
            }
        }
        # endregion
        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private string _path;
        private static IEnumerable<System.IO.FileInfo> lastQuery;

       public NotifyWatch()
       {
           fileNotificationsProperty.CollectionChanged += fileNotificationsProperty_CollectionChanged;
          InitializeComponent();
       }

       void fileNotificationsProperty_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
       {
           PropertyChanged(this, new PropertyChangedEventArgs("FileNotifications"));
       }  

        private void onClickStartWatch(object sender, RoutedEventArgs e)
        {
            _path = pathBox.Text;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 60);
            dispatcherTimer.Start();
        }
        private void onClickStopWatch(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
        }
        private void onClickClearWatch(object sender, RoutedEventArgs e)
        {
            FileNotifications.Clear();
        }
        private async void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            bool _dirExists = Directory.Exists(_path);
            if (_dirExists)
            {
                List<FileInfo> _query = await TaskOfTResult_MethodAsync();
                foreach (var v in _query)
                {
                    FileNotifications.Add(v);
                }
            }
        }

        async Task<List<FileInfo>> TaskOfTResult_MethodAsync()
        {
            DirectoryInfo dInfo = new DirectoryInfo(_path);
            List<FileInfo> queryList1Only = new List<FileInfo>();
            if (lastQuery != null)
            {
                IEnumerable<System.IO.FileInfo> list1 = dInfo.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
                FileNameCompare fileComparer = new FileNameCompare();
                queryList1Only = ((from file in list1 select file).Except(lastQuery, fileComparer)).ToList();
                lastQuery = list1;
            }
            else
            {
                lastQuery = dInfo.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            }
            return queryList1Only;
        }

        private void onClickDirSelect(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                pathBox.Text = dialog.SelectedPath;
            }
        }

        private void onClickOpenElem(object sender, MouseButtonEventArgs e)
        {
            Border ctrl = (Border)sender;
            FileInfo fi = (FileInfo)ctrl.DataContext;
            string argument = @"/select, " + fi.FullName;
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private void onClickDeleteElem(object sender, MouseButtonEventArgs e)
        {
            Border ctrl = (Border)sender;
            FileInfo fi = (FileInfo)ctrl.DataContext;
            FileNotifications.Remove(fi);
        }



        public event PropertyChangedEventHandler PropertyChanged;
    }

    class FileNameCompare : IEqualityComparer<FileInfo>
    {
        public FileNameCompare() { }

        public bool Equals(System.IO.FileInfo f1, System.IO.FileInfo f2)
        {
            return (f1.FullName == f2.FullName && f1.LastWriteTime==f2.LastWriteTime);
        }

        // Return a hash that reflects the comparison criteria. According to the  
        // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must 
        // also be equal. Because equality as defined here is a simple value equality, not 
        // reference identity, it is possible that two or more objects will produce the same 
        // hash code. 
        public int GetHashCode(System.IO.FileInfo fi)
        {
            string s = String.Format("{0}{1}", fi.Name, fi.Length);
            return s.GetHashCode();
        }
    }
}
