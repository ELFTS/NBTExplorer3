using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NBTExplorer.Wpf.ViewModels;

namespace NBTExplorer.Wpf
{
    public partial class MainWindow : Window
    {
        private NodeViewModel _lastSelectedNode;

        public MainWindow()
        {
            InitializeComponent();
            
            // Handle command line arguments
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                string[] paths = new string[args.Length - 1];
                Array.Copy(args, 1, paths, 0, paths.Length);
                OpenPaths(paths);
            }
            else
            {
                // Open Minecraft save folder by default
                OpenMinecraftSaveFolder();
            }
        }
        
        private void OpenPaths(string[] paths)
        {
            if (DataContext is MainViewModel vm)
            {
                foreach (var path in paths)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        vm.LoadFolder(path);
                    }
                    else if (System.IO.File.Exists(path))
                    {
                        vm.LoadFile(path);
                    }
                }
            }
        }
        
        private void OpenMinecraftSaveFolder()
        {
            try
            {
                string minecraftPath = Environment.ExpandEnvironmentVariables("%APPDATA%");
                if (!System.IO.Directory.Exists(minecraftPath))
                {
                    minecraftPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                }

                minecraftPath = System.IO.Path.Combine(minecraftPath, ".minecraft");
                minecraftPath = System.IO.Path.Combine(minecraftPath, "saves");

                if (System.IO.Directory.Exists(minecraftPath))
                {
                    if (DataContext is MainViewModel vm)
                    {
                        vm.LoadFolder(minecraftPath);
                    }
                }
                // No fallback to current directory - just show empty tree if Minecraft folder doesn't exist
            }
            catch (Exception)
            {
                // Ignore if Minecraft folder can't be found
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel vm)
            {
                var selectedNode = e.NewValue as NodeViewModel;
                vm.SelectedNode = selectedNode;
                UpdateStatusBar(selectedNode);
                UpdateDetailPanel(selectedNode);
            }
        }

        private void UpdateStatusBar(NodeViewModel selectedNode)
        {
            if (selectedNode != null)
            {
                var node = selectedNode.Node;
                if (node is NBTExplorer.Model.TagDataNode tagNode)
                {
                    var tagType = tagNode.Tag.GetTagType();
                    NodeInfo.Text = $"{node.NodePathName} - {tagType.ToString()} Tag";
                }
                else if (node is NBTExplorer.Model.RegionFileDataNode)
                {
                    NodeInfo.Text = $"{node.NodePathName} - Region File";
                }
                else if (node is NBTExplorer.Model.DirectoryDataNode)
                {
                    NodeInfo.Text = $"{node.NodePathName} - Directory";
                }
                else
                {
                    NodeInfo.Text = $"{node.NodePathName}";
                }
                StatusText.Text = "Ready";
            }
            else
            {
                NodeInfo.Text = "No node selected";
                StatusText.Text = "Ready";
            }
        }

        private void UpdateDetailPanel(NodeViewModel selectedNode)
        {
            if (selectedNode != null)
            {
                var node = selectedNode.Node;
                DetailName.Text = $"Name: {node.NodePathName}";
                
                if (node is NBTExplorer.Model.TagDataNode tagNode)
                {
                    var tagType = tagNode.Tag.GetTagType();
                    DetailType.Text = $"Type: {tagType.ToString()} Tag";
                    DetailPath.Text = $"Path: {node.NodePath}";
                    DetailValue.Text = $"Value: {tagNode.Tag.ToString()}";
                }
                else if (node is NBTExplorer.Model.RegionFileDataNode)
                {
                    DetailType.Text = "Type: Region File";
                    DetailPath.Text = $"Path: {node.NodePath}";
                    DetailValue.Text = "Value: N/A";
                }
                else if (node is NBTExplorer.Model.DirectoryDataNode)
                {
                    DetailType.Text = "Type: Directory";
                    DetailPath.Text = $"Path: {node.NodePath}";
                    DetailValue.Text = "Value: N/A";
                }
                else
                {
                    DetailType.Text = $"Type: {node.GetType().Name}";
                    DetailPath.Text = $"Path: {node.NodePath}";
                    DetailValue.Text = "Value: N/A";
                }
            }
            else
            {
                DetailName.Text = "Name: N/A";
                DetailType.Text = "Type: N/A";
                DetailPath.Text = "Path: N/A";
                DetailValue.Text = "Value: N/A";
            }
        }

        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(NodeViewModel)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] items = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (DataContext is MainViewModel vm)
                {
                    foreach (var item in items)
                    {
                        if (System.IO.Directory.Exists(item))
                        {
                            vm.LoadFolder(item);
                        }
                        else if (System.IO.File.Exists(item))
                        {
                            vm.LoadFile(item);
                        }
                    }
                }
            }
            else if (e.Data.GetDataPresent(typeof(NodeViewModel)))
            {
                // 处理节点间拖放
                var draggedNode = e.Data.GetData(typeof(NodeViewModel)) as NodeViewModel;
                var treeViewItem = FindParent<System.Windows.Controls.TreeViewItem>(e.OriginalSource as DependencyObject);
                var targetNode = treeViewItem?.DataContext as NodeViewModel;
                
                if (draggedNode != null && targetNode != null && draggedNode != targetNode)
                {
                    // 实现节点移动逻辑
                    try
                    {
                        // 首先剪切节点
                        draggedNode.CutCommand.Execute(null);
                        // 然后粘贴到目标节点
                        targetNode.PasteCommand.Execute(null);
                        // 刷新目标节点的子节点
                        if (targetNode.IsExpanded)
                        {
                            // 这里可以添加刷新逻辑
                        }
                        else
                        {
                            targetNode.IsExpanded = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error moving node: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
        }

        private void TreeView_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var treeView = sender as System.Windows.Controls.TreeView;
                var hitTestResult = System.Windows.Media.VisualTreeHelper.HitTest(treeView, e.GetPosition(treeView));
                
                if (hitTestResult != null)
                {
                    var treeViewItem = FindParent<TreeViewItem>(hitTestResult.VisualHit);
                    if (treeViewItem != null)
                    {
                        var nodeViewModel = treeViewItem.DataContext as NodeViewModel;
                        if (nodeViewModel != null)
                        {
                            DataObject dataObject = new DataObject(typeof(NodeViewModel), nodeViewModel);
                            System.Windows.DragDrop.DoDragDrop(treeViewItem, dataObject, DragDropEffects.Move);
                        }
                    }
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                {
                    return parent;
                }
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var treeViewItem = sender as System.Windows.Controls.TreeViewItem;
            if (treeViewItem != null)
            {
                var nodeViewModel = treeViewItem.DataContext as NodeViewModel;
                if (nodeViewModel != null)
                {
                    // 检查是否按下了Ctrl键
                    if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
                    {
                        // Ctrl+点击：切换选中状态
                        nodeViewModel.IsSelected = !nodeViewModel.IsSelected;
                        _lastSelectedNode = nodeViewModel;
                        e.Handled = true;
                    }
                    else if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Shift)
                    {
                        // Shift+点击：选择范围
                        if (_lastSelectedNode != null && _lastSelectedNode.Parent == nodeViewModel.Parent)
                        {
                            // 只在同级节点之间实现范围选择
                            var mainViewModel = DataContext as MainViewModel;
                            if (mainViewModel != null)
                            {
                                // 取消所有其他节点的选中状态
                                DeselectAllNodes(mainViewModel.RootNodes);
                                
                                // 选择从上次选中的节点到当前节点的所有节点
                                SelectRange(_lastSelectedNode, nodeViewModel);
                            }
                        }
                        e.Handled = true;
                    }
                    else
                    {
                        // 普通点击：取消其他选中，只选中当前节点
                        var mainViewModel = DataContext as MainViewModel;
                        if (mainViewModel != null)
                        {
                            // 取消所有其他节点的选中状态
                            DeselectAllNodes(mainViewModel.RootNodes);
                            // 选中当前节点
                            nodeViewModel.IsSelected = true;
                            _lastSelectedNode = nodeViewModel;
                        }
                    }
                }
            }
        }

        private void SelectRange(NodeViewModel startNode, NodeViewModel endNode)
        {
            if (startNode == null || endNode == null || startNode.Parent == null)
                return;

            var parent = startNode.Parent;
            var children = parent.Children;
            
            int startIndex = children.IndexOf(startNode);
            int endIndex = children.IndexOf(endNode);
            
            if (startIndex == -1 || endIndex == -1)
                return;

            // 确保startIndex <= endIndex
            if (startIndex > endIndex)
            {
                var temp = startIndex;
                startIndex = endIndex;
                endIndex = temp;
            }

            // 选择范围内的所有节点
            for (int i = startIndex; i <= endIndex; i++)
            {
                var node = children[i];
                if (node != null)
                {
                    node.IsSelected = true;
                }
            }
        }

        private void DeselectAllNodes(System.Collections.Generic.IEnumerable<NodeViewModel> nodes)
        {
            if (nodes == null)
                return;

            foreach (var node in nodes)
            {
                if (node != null)
                {
                    node.IsSelected = false;
                    if (node.Children != null)
                    {
                        DeselectAllNodes(node.Children);
                    }
                }
            }
        }
    }
}