using System;
using System.Windows;
using NBTExplorer.Model;
using NBTExplorer.Model.Search;
using NBTExplorer.Wpf.Services;
using NBTExplorer.Wpf.Views;
using Substrate.Nbt;

namespace NBTExplorer.Wpf.ViewModels
{
    public class ContainerRuleSearchState : ISearchState
    {
        private readonly MainViewModel _mainViewModel;
        private readonly RootRule _rootRule;
        private readonly FindReplaceViewModel _findReplaceViewModel;
        
        public ContainerRuleSearchState(MainViewModel mainViewModel, RootRule rootRule)
        {
            _mainViewModel = mainViewModel;
            _rootRule = rootRule;
            _findReplaceViewModel = new FindReplaceViewModel();
            
            TerminateOnDiscover = true;
            ProgressRate = 0.5f;
        }
        
        public DataNode RootNode { get; set; }
        public System.Collections.Generic.IEnumerator<DataNode> State { get; set; }
        public bool TerminateOnDiscover { get; set; }
        public bool IsTerminated { get; set; }
        public float ProgressRate { get; set; }
        public bool ReplaceAll { get; set; }
        
        public void InvokeDiscoverCallback(DataNode node)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _mainViewModel.SelectNode(node);
                _findReplaceViewModel.CurrentFindNode = node;
                
                if (ReplaceAll)
                {
                    _findReplaceViewModel.ReplaceCurrent();
                }
            });
        }
        
        public void InvokeProgressCallback(DataNode node)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update cancel search window with current path
                var cancelWindow = Application.Current.Windows.OfType<CancelSearchWindow>().FirstOrDefault();
                if (cancelWindow != null)
                {
                    cancelWindow.SearchPath = node.NodePath;
                }
            });
        }
        
        public void InvokeCollapseCallback(DataNode node)
        {
            // No action needed for collapse
        }
        
        public void InvokeEndCallback(DataNode node)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("End of Results", "Search Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
        
        public bool TestNode(DataNode node)
        {
            if (node is TagCompoundDataNode compoundNode)
            {
                var matches = new System.Collections.Generic.List<TagDataNode>();
                return _rootRule.Matches(compoundNode, matches);
            }
            return false;
        }
    }
}