using System.Windows;
using NBTExplorer.Wpf.Services;

namespace NBTExplorer.Wpf.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;

        public SettingsWindow(SettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
        }

        private void ClearRecentFiles_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.Settings.RecentFiles.Clear();
            _settingsService.Save();
            MessageBox.Show(this, "Recent files list cleared.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
