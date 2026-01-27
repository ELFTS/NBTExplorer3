using System;
using System.Threading.Tasks;
using System.Windows;
using NBTExplorer.Model;
using NBTExplorer.Wpf.ViewModels;

namespace NBTExplorer.Wpf.Services
{
    public class WpfSearchState : NameValueSearchState
    {
        private readonly MainViewModel _viewModel;

        public WpfSearchState(MainViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override void InvokeDiscoverCallback(DataNode node)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _viewModel.SelectNode(node);
            });
        }

        public override void InvokeProgressCallback(DataNode node)
        {
            // Update status bar with search progress
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    // Assuming MainWindow has a method to update status bar
                    // For now, we'll just set the status text directly if possible
                    var statusText = mainWindow.FindName("StatusText") as System.Windows.Controls.TextBlock;
                    if (statusText != null)
                    {
                        statusText.Text = $"Searching: {node.NodePathName}";
                    }
                }
            });
        }

        public override void InvokeCollapseCallback(DataNode node)
        {
            // Handle UI collapse if needed
            // This would typically collapse the tree view nodes below the specified node
            // For now, we'll just log this action
            Console.WriteLine($"Collapse callback for node: {node.NodePathName}");
        }

        public override void InvokeEndCallback(DataNode node)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("End of search reached.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }
}
