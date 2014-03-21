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
using System.Yaml;

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
        public List<FileTypeFilter> filters;
        public List<YamlNode> settings;
        private bool isWatching = false;
        private string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        public Dictionary<string, FileInfo> lastQuery;

        public WatchHolder()
        {
            bool pathLoaded = InitPath();
            bool settingsLoaded = InitSettings();
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            FileNotifications = new SortableObservableCollection<ValueDifference<FileInfo>>();
        }
        public bool InitPath()
        {
            bool result = true;
            if (File.Exists("watch_list.yml"))
            {
                List<YamlNode> _path = YamlNode.FromYamlFile("watch_list.yml").ToList();
                YamlMapping mapping = (YamlMapping)_path[0];
                YamlScalar filtersMapping = mapping.First(x => ((YamlScalar)x.Key).Value == "watched").Value as YamlScalar;
                Path = filtersMapping.Value;
            }
            else
            {
                result = false;
            }
            return result;
        }
        public bool InitSettings()
        {
            bool result = true;
            try
            {
                filters = new List<FileTypeFilter>();
                if (File.Exists("settings.yml"))
                {

                    settings = YamlNode.FromYamlFile("settings.yml").ToList();
                    YamlMapping mapping = (YamlMapping)settings[0];
                    YamlMapping filtersMapping = mapping.First(x => ((YamlScalar)x.Key).Value == "filters").Value as YamlMapping;
                    foreach (KeyValuePair<YamlNode, YamlNode> _filter in filtersMapping)
                    {
                        YamlScalar _keyNode = _filter.Key as YamlScalar;
                        YamlSequence _valueNode = _filter.Value as YamlSequence;
                        List<string> extensions = _valueNode.Select(x => ("."+((YamlScalar)x).Value)).ToList();
                        filters.Add(new FileTypeFilter(_keyNode.Value, true, extensions));
                    }
                }
                else
                {
                    throw new FileNotFoundException("the settings file was not found, file missing", AppDomain.CurrentDomain.BaseDirectory + "settings.yml");
                }
            }

            catch
            {
                result = false;
            }
            return result;
        }

        public void StartWatching()
        {
            bool _dirExists = Directory.Exists(_path);
            if (isWatching)
            {
                StopWatching();
            }
            if (_dirExists)
            {
                DirectoryInfo dInfo = new DirectoryInfo(_path);
                lastQuery = dInfo.GetFilesByExtensions(filters.SelectMany(x => x.Extensions).ToArray()).ToDictionary(x => x.FullName);
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                dispatcherTimer.Start();
                isWatching = true;
            }
        }
        
        public void StopWatching()
        {
            dispatcherTimer.Stop();
            isWatching = false;
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
            Dictionary<string, FileInfo> fileList = dInfo.GetFilesByExtensions(filters.SelectMany(x => x.Extensions).ToArray()).ToDictionary(x => x.FullName);
            FileCompare _fileComparer = new FileCompare();
            diff = fileList.CompareDictionary<string, FileInfo>(lastQuery, _fileComparer);
            lastQuery = fileList;
            return diff;
        }
        public ICommand StartWatchingCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => (Path != null && Path != ""),
                    CommandAction = () =>
                    {
                        StartWatching();
                    }
                };
            }
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
