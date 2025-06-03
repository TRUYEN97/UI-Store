using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace UiStore.Services
{
    internal class Logger
    {
        public ObservableCollection<string> LogLines { get; } = new ObservableCollection<string>();

        public void AddLogLine(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} -> {message}");
                while (LogLines.Count > 10)
                {
                    LogLines.RemoveAt(0);
                }
            });
        }
    }
}
