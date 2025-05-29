
using System.IO;
using System.Threading;
using System.Windows;
using UiStore.Configs;
using UiStore.View;

namespace UiStore
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                const string appName = "Global\\UiStoreApp";
                bool createdNew;

                _mutex = new Mutex(true, appName, out createdNew);

                if (!createdNew)
                {
                    //MessageBox.Show("Ui Store đã được mở rồi!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    File.WriteAllText("show.signal", "show");
                    Shutdown();
                    return;
                }
                base.OnStartup(e);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi khởi động: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }

    }
}
