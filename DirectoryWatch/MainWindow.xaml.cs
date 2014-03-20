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
using Hardcodet.Wpf.TaskbarNotification;
using System.IO;
using System.Diagnostics;

namespace DirectoryWatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private string _path;
        private static Dictionary<string,FileInfo> lastQuery;
        

        public MainWindow()
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            FileNotifications = new SortableObservableCollection<ValueDifference<FileInfo>>();
            InitializeComponent();

        }

        private void onClickStartWatch(object sender, RoutedEventArgs e)
        {
            bool _dirExists = Directory.Exists(_path);
            if (_dirExists)
            {
                DirectoryInfo dInfo = new DirectoryInfo(_path);
                lastQuery = dInfo.GetFiles("*.*", System.IO.SearchOption.AllDirectories).ToDictionary(x => x.FullName);
                _path = pathBox.Text;
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                dispatcherTimer.Start();
            }
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
                List<ValueDifference<FileInfo>> _query = await TaskOfTResult_MethodAsync();
                foreach (var v in _query)
                {              
                    FileNotifications.Add(v);
                    TaskbarIcon TI = (TaskbarIcon)FindResource("NotifyIcon");
                    FancyBalloon balloon = new FancyBalloon();
                    balloon.BalloonText = "Latest change";
                    balloon.LatestNotify.Text = v.ValueInfo.Name;
                    //show balloon and close it after 4 seconds
                    TI.ShowCustomBalloon(balloon, PopupAnimation.Slide, 4000);
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

        private void onClickDirSelect(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                pathBox.Text = dialog.SelectedPath;
            }
            _path = pathBox.Text;
        }

        private void onClickOpenElem(object sender, MouseButtonEventArgs e)
        {
            Border ctrl = (Border)sender;
            ValueDifference<FileInfo> fi = (ValueDifference<FileInfo>)ctrl.DataContext;
            string argument = @"/select, " + fi.valueInfo.FullName;
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private void onClickDeleteElem(object sender, MouseButtonEventArgs e)
        {
            Border ctrl = (Border)sender;
            ValueDifference<FileInfo> fi = (ValueDifference<FileInfo>)ctrl.DataContext;
            FileNotifications.Remove(fi);
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
                return new DelegateCommand {CommandAction = () => Application.Current.Shutdown()};
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


    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null  || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    class FileCompare : IEqualityComparer<FileInfo>
    {
        public FileCompare() { }

        public bool Equals(System.IO.FileInfo f1, System.IO.FileInfo f2)
        {
            return (f1.LastWriteTime==f2.LastWriteTime);
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
