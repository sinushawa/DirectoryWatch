using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using System.IO;
using System.Diagnostics;

namespace DirectoryWatch
{
    public class WatchHolder : FrameworkElement
    {
        # region FileNotifications dependency definition
        public static readonly DependencyProperty FileNotificationsProperty = DependencyProperty.Register("FileNotifications", typeof(SortableObservableCollection<ValueDifference<FileInfo>>), typeof(MainWindow), new FrameworkPropertyMetadata(default(SortableObservableCollection<ValueDifference<FileInfo>>), new PropertyChangedCallback(onCollectionChanged)));
        public SortableObservableCollection<ValueDifference<FileInfo>> FileNotifications
        {
            get
            {
                return (SortableObservableCollection<ValueDifference<FileInfo>>)GetValue(FileNotificationsProperty);
            }
            set
            {
                SetValue(FileNotificationsProperty, value);
            }
        }
        private static void onCollectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
        # endregion
        public DispatcherTimer dispatcherTimer;
        private string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        public Dictionary<string, FileInfo> lastQuery;

        public WatchHolder()
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            FileNotifications = new SortableObservableCollection<ValueDifference<FileInfo>>();
        }
        public void StartWatching()
        {
            bool _dirExists = Directory.Exists(_path);
            if (_dirExists)
            {
                DirectoryInfo dInfo = new DirectoryInfo(_path);
                lastQuery = dInfo.GetFiles("*.*", System.IO.SearchOption.AllDirectories).ToDictionary(x => x.FullName);
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                dispatcherTimer.Start();
            }
        }
        public void StopWatching()
        {
            dispatcherTimer.Stop();
        }
        public void ClearWatch()
        {
            FileNotifications.Clear();
        }
        private async void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            bool _dirExists = Directory.Exists(_path);
            if (_dirExists)
            {
                List<ValueDifference<FileInfo>> _query = await TaskOfTResult_MethodAsync();
                foreach (var v in _query)
                {
                    FileNotifications.Add(v);
                    TaskbarIcon TI = (TaskbarIcon)FindResource("NotifyIcon");
                    FancyBalloon balloon = new FancyBalloon();
                    balloon.BalloonText = "Latest change";
                    balloon.LatestNotify.Text = v.ValueInfo.Name;
                    //show balloon and close it after 20 seconds
                    TI.ShowCustomBalloon(balloon, PopupAnimation.Slide, 20000);
                }
            }
        }

        async Task<List<ValueDifference<FileInfo>>> TaskOfTResult_MethodAsync()
        {
            DirectoryInfo dInfo = new DirectoryInfo(_path);
            List<ValueDifference<FileInfo>> diff = new List<ValueDifference<FileInfo>>();
            Dictionary<string, FileInfo> fileList = dInfo.GetFiles("*.*", System.IO.SearchOption.AllDirectories).ToDictionary(x => x.FullName);
            FileCompare _fileComparer = new FileCompare();
            diff = fileList.CompareDictionary<string, FileInfo>(lastQuery, _fileComparer);
            lastQuery = fileList;
            return diff;
        }
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => (Application.Current.MainWindow == null || Application.Current.MainWindow.IsVisible != true),
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow = new MainWindow();
                        Application.Current.MainWindow.Show();
                    }
                };
            }
        }

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Application.Current.MainWindow.Close(),
                    CanExecuteFunc = () => Application.Current.MainWindow != null
                };
            }
        }


        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }

        public string GetLastNotification
        {
            get
            {
                string result = "";
                if (FileNotifications.Count > 0)
                {
                    result = FileNotifications.Last().ValueInfo.Name;
                }
                return result;
            }
        }
    }
}
