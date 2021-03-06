﻿using System;
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
using System.Yaml.Serialization;

namespace DirectoryWatch
{
    public class WatchHolder : FrameworkElement
    {
        # region FileNotifications dependency definition
        public static readonly DependencyProperty FileNotificationsProperty = DependencyProperty.Register("FileNotifications", typeof(SortableObservableCollection<ValueDifference<FileInfoHolder>>), typeof(MainWindow), new FrameworkPropertyMetadata(default(SortableObservableCollection<ValueDifference<FileInfoHolder>>), new PropertyChangedCallback(onFileNotificationsChanged)));
        public SortableObservableCollection<ValueDifference<FileInfoHolder>> FileNotifications
        {
            get
            {
                return (SortableObservableCollection<ValueDifference<FileInfoHolder>>)GetValue(FileNotificationsProperty);
            }
            set
            {
                SetValue(FileNotificationsProperty, value);
            }
        }
        private static void onFileNotificationsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
        # endregion
        # region PathFolder dependency definition
        public static readonly DependencyProperty PathFolderProperty = DependencyProperty.Register("PathFolder", typeof(string), typeof(MainWindow), new FrameworkPropertyMetadata(default(string)));
        public string PathFolder
        {
            get
            {
                return (string)GetValue(PathFolderProperty);
            }
            set
            {
                SetValue(PathFolderProperty, value);
            }
        }
        # endregion
        # region filters dependency definition
        public static readonly DependencyProperty filtersProperty = DependencyProperty.Register("Filters", typeof(SortableObservableCollection<FileTypeFilter>), typeof(MainWindow), new FrameworkPropertyMetadata(default(SortableObservableCollection<FileTypeFilter>), new PropertyChangedCallback(onFiltersChanged)));
        public SortableObservableCollection<FileTypeFilter> Filters
        {
            get
            {
                return (SortableObservableCollection<FileTypeFilter>)GetValue(filtersProperty);
            }
            set
            {
                SetValue(filtersProperty, value);
            }
        }
        private static void onFiltersChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
        # endregion
        public DispatcherTimer dispatcherTimer;
        public List<YamlNode> settings;
        public Dictionary<FileState,BitmapImage> icons;
        private bool isWatching = false;
        public Dictionary<string, FileInfoHolder> lastQuery;

        public WatchHolder()
        {
            bool settingsLoaded = InitSettings();
            InitImage();
            InitWatch();
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            FileNotifications = new SortableObservableCollection<ValueDifference<FileInfoHolder>>();
        }
        
        public void InitImage()
        {
            icons = new Dictionary<FileState, BitmapImage>();
            BitmapImage newFile = new BitmapImage();
            newFile.BeginInit();
            newFile.UriSource = new Uri("pack://application:,,,/DirectoryWatch;component/page_add.png");
            newFile.EndInit();
            icons.Add(FileState.IsNew,newFile);
            BitmapImage delFile = new BitmapImage();
            delFile.BeginInit();
            delFile.UriSource = new Uri("pack://application:,,,/DirectoryWatch;component/page_remove.png");
            delFile.EndInit();
            icons.Add(FileState.IsDeleted, delFile);
            BitmapImage modFile = new BitmapImage();
            modFile.BeginInit();
            modFile.UriSource = new Uri("pack://application:,,,/DirectoryWatch;component/page_edit.png");
            modFile.EndInit();
            icons.Add(FileState.IsModified, modFile);
        }
        public bool InitSettings()
        {
            bool result = true;
            try
            {
                Filters = new SortableObservableCollection<FileTypeFilter>();
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
                        Filters.Add(new FileTypeFilter(_keyNode.Value, true, extensions));
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
        public void InitWatch()
        {
            if (File.Exists("watch_list.yml"))
            {
                YamlSerializer serializer = new YamlSerializer();
                WatchSave watchSave = (WatchSave)(serializer.DeserializeFromFile("watch_list.yml", new Type[] { (typeof(WatchSave)) })[0]);
                PathFolder = watchSave.watchedFolder;
                Filters = watchSave.filters;
            }
        }
        public void StartWatching()
        {
            bool _dirExists = Directory.Exists(PathFolder);
            if (isWatching)
            {
                StopWatching();
            }
            if (_dirExists)
            {
                DirectoryInfo dInfo = new DirectoryInfo(PathFolder);
                lastQuery = dInfo.GetFilesByExtensions(Filters.Where(x=> x.IsEnabled).SelectMany(x => x.Extensions).ToArray()).ToDictionary(x => x.FullName, y=> y.ToFileInfoHolder());
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
        public void SaveFilesInfo()
        {
            WatchSave watchSave = new WatchSave(PathFolder, Filters);
            YamlSerializer serializer = new YamlSerializer();
            serializer.SerializeToFile("watch_list.yml", watchSave);
            serializer.SerializeToFile("query.yml", lastQuery);
        }
        private async void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            bool _dirExists = Directory.Exists(PathFolder);
            if (_dirExists)
            {
                List<ValueDifference<FileInfoHolder>> _query = await TaskOfTResult_MethodAsync();
                foreach (var v in _query)
                {
                    FileNotifications.Add(v);
                    TaskbarIcon TI = (TaskbarIcon)FindResource("NotifyIcon");
                    FancyBalloon balloon = new FancyBalloon();
                    balloon.latestType.Source = icons[v.State];
                    balloon.LatestNotify.Text = v.ValueInfo.Name;
                    //show balloon and close it after 20 seconds
                    TI.ShowCustomBalloon(balloon, PopupAnimation.Slide, 20000);
                }
            }
        }

        async Task<List<ValueDifference<FileInfoHolder>>> TaskOfTResult_MethodAsync()
        {
            DirectoryInfo dInfo = new DirectoryInfo(PathFolder);
            List<ValueDifference<FileInfoHolder>> diff = new List<ValueDifference<FileInfoHolder>>();
            Dictionary<string, FileInfoHolder> fileList = dInfo.GetFilesByExtensions(Filters.Where(x => x.IsEnabled).SelectMany(x => x.Extensions).ToArray()).ToDictionary(x => x.FullName, y=> y.ToFileInfoHolder());
            FileCompare _fileComparer = new FileCompare();
            diff = fileList.CompareDictionary<string, FileInfoHolder>(lastQuery, _fileComparer);
            lastQuery = fileList;
            return diff;
        }
        public ICommand StartWatchingCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => (PathFolder != null && PathFolder != ""),
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
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        SaveFilesInfo();
                        Application.Current.Shutdown();
                    }
                };
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
