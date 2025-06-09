using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace UiStore.Services
{
    internal class Logger
    {
        private readonly ObservableCollection<string> logLines;
        private readonly string name;

        public Logger(ObservableCollection<string> logLines, string name)
        {
            this.logLines = logLines;
            this.name = name;
        }

        public Logger CreateNew(string name)
        {
            return new Logger(logLines, name);
        }

        public void AddLogLine(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                logLines.Add($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} [{name?.Trim()}] -> {message}");
                while (logLines.Count > 10)
                {
                    logLines.RemoveAt(0);
                }
            });
        }
    }
}
