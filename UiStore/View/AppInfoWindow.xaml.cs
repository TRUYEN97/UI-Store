using System.Windows;
using UiStore.ViewModel;

namespace UiStore.View
{
    /// <summary>
    /// Interaction logic for AppInfoWindow.xaml
    /// </summary>
    public partial class AppInfoWindow : Window
    {

        internal AppInfoWindow(AppViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
