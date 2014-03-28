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
        private WatchHolder watchHolder;

        public MainWindow()
        {
            watchHolder = ((App)Application.Current).watchHolder;
            InitializeComponent();
        }

        private void onClickStartWatch(object sender, RoutedEventArgs e)
        {
            watchHolder.StartWatching();
        }
        private void onClickStopWatch(object sender, RoutedEventArgs e)
        {
            watchHolder.StopWatching();
        }
        private void onClickClearWatch(object sender, RoutedEventArgs e)
        {
            watchHolder.ClearWatch();
        }

        private void onClickDirSelect(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                pathBox.Text = dialog.SelectedPath;
            }
            watchHolder.PathFolder = pathBox.Text;
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
            ValueDifference<FileInfoHolder> fi = (ValueDifference<FileInfoHolder>)ctrl.DataContext;
            watchHolder.FileNotifications.Remove(fi);
        }

        private void onFilterChecked(object sender, RoutedEventArgs e)
        {
            CheckBox ctrl = (CheckBox)sender;
        }
        private void onFilterUnchecked(object sender, RoutedEventArgs e)
        {
            CheckBox ctrl = (CheckBox)sender;
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
}
