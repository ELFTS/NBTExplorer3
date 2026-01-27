using System.Windows;
using System.Windows.Controls;
using NBTExplorer.Wpf.ViewModels;

namespace NBTExplorer.Wpf.Views
{
    public partial class FindReplaceWindow : Window
    {
        public FindReplaceWindow()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is FindReplaceViewModel viewModel)
            {
                viewModel.SelectedRule = e.NewValue as RuleViewModel;
            }
        }

        private void ReplaceTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is FindReplaceViewModel viewModel)
            {
                viewModel.SelectedReplaceNode = e.NewValue as NodeViewModel;
            }
        }
    }
}