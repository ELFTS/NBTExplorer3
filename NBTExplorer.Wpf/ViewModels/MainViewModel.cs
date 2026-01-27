using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Input;
using NBTExplorer.Model;
using NBTExplorer.Wpf.Controllers;
using NBTExplorer.Wpf.Services;
using NBTExplorer.Wpf.Views;

namespace NBTExplorer.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<NodeViewModel> _rootNodes;
        private NodeViewModel _selectedNode;
        private ObservableCollection<NodeViewModel> _selectedNodes;
        private SearchWorker _searchWorker;
        private SettingsService _settingsService;
        private WpfSearchState _searchState;

        public MainViewModel()
        {
            _settingsService = new SettingsService();
            _rootNodes = new ObservableCollection<NodeViewModel>();
            _selectedNode = null;
            _selectedNodes = new ObservableCollection<NodeViewModel>();
            _searchWorker = null;
            _searchState = null;
            OpenFileCommand = new RelayCommand(OpenFile);
            OpenFolderCommand = new RelayCommand(OpenFolder);
            OpenMinecraftSaveFolderCommand = new RelayCommand(OpenMinecraftSaveFolder);
            OpenRecentCommand = new RelayCommand(OpenRecent);
            SaveCommand = new RelayCommand(Save);
            ExitCommand = new RelayCommand(Exit);

            CutNodeCommand = new RelayCommand(p => {
                if (SelectedNodes.Count > 1)
                    new NodeTreeController(this).BatchCutNodes(SelectedNodes);
                else
                    SelectedNode?.CutCommand.Execute(null);
            }, p => {
                if (SelectedNodes.Count > 1)
                    return SelectedNodes.Any(n => n.CutCommand.CanExecute(null) == true);
                else
                    return SelectedNode?.CutCommand.CanExecute(null) == true;
            });
            CopyNodeCommand = new RelayCommand(p => {
                if (SelectedNodes.Count > 1)
                    new NodeTreeController(this).BatchCopyNodes(SelectedNodes);
                else
                    SelectedNode?.CopyCommand.Execute(null);
            }, p => {
                if (SelectedNodes.Count > 1)
                    return SelectedNodes.Any(n => n.CopyCommand.CanExecute(null) == true);
                else
                    return SelectedNode?.CopyCommand.CanExecute(null) == true;
            });
            PasteNodeCommand = new RelayCommand(p => SelectedNode?.PasteCommand.Execute(null), p => SelectedNode?.PasteCommand.CanExecute(null) == true);
            RenameNodeCommand = new RelayCommand(p => SelectedNode?.RenameCommand.Execute(null), p => SelectedNode?.RenameCommand.CanExecute(null) == true);
            EditValueNodeCommand = new RelayCommand(p => SelectedNode?.EditValueCommand.Execute(null), p => SelectedNode?.EditValueCommand.CanExecute(null) == true);
            DeleteNodeCommand = new RelayCommand(p => {
                if (SelectedNodes.Count > 1)
                    new NodeTreeController(this).BatchDeleteNodes(SelectedNodes);
                else
                    SelectedNode?.DeleteCommand.Execute(null);
            }, p => {
                if (SelectedNodes.Count > 1)
                    return SelectedNodes.Any(n => n.DeleteCommand.CanExecute(null) == true);
                else
                    return SelectedNode?.DeleteCommand.CanExecute(null) == true;
            });
            RefreshNodeCommand = new RelayCommand(p => SelectedNode?.RefreshCommand.Execute(null), p => SelectedNode?.RefreshCommand.CanExecute(null) == true);
            CreateTagNodeCommand = new RelayCommand(p => SelectedNode?.CreateTagCommand.Execute(p), p => SelectedNode?.CreateTagCommand.CanExecute(p) == true);
            MoveUpNodeCommand = new RelayCommand(p => SelectedNode?.MoveUpCommand.Execute(null), p => SelectedNode?.MoveUpCommand.CanExecute(null) == true);
            MoveDownNodeCommand = new RelayCommand(p => SelectedNode?.MoveDownCommand.Execute(null), p => SelectedNode?.MoveDownCommand.CanExecute(null) == true);
            OpenInExplorerCommand = new RelayCommand(p => SelectedNode?.OpenInExplorerCommand.Execute(null), p => SelectedNode?.OpenInExplorerCommand.CanExecute(null) == true);

            FindCommand = new RelayCommand(Find);
            FindNextCommand = new RelayCommand(FindNext, p => _searchState != null);
            FindBlockCommand = new RelayCommand(FindBlock, p => SelectedNode?.Node != null);
            FindReplaceCommand = new RelayCommand(FindReplace);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            AboutCommand = new RelayCommand(About);
        }

        public ObservableCollection<NodeViewModel> RootNodes => _rootNodes;
        public List<string> RecentFiles => _settingsService.Settings.RecentFiles;
        public List<RecentItem> RecentFilesWithNumbers => GetRecentItemsWithNumbers(_settingsService.Settings.RecentFiles);
        public List<RecentItem> RecentDirectoriesWithNumbers => GetRecentItemsWithNumbers(_settingsService.Settings.RecentDirectories);
        public ObservableCollection<NodeViewModel> SelectedNodes => _selectedNodes;
        public SettingsService SettingsService => _settingsService;

        public class RecentItem
        {
            public string Display { get; set; }
            public string Path { get; set; }
        }

        private List<RecentItem> GetRecentItemsWithNumbers(List<string> items)
        {
            var result = new List<RecentItem>();
            for (int i = 0; i < items.Count; i++)
            {
                // Add number prefix (e.g., "&1 " for Alt+1 shortcut)
                result.Add(new RecentItem
                {
                    Display = $"&{i + 1} {items[i]}",
                    Path = items[i]
                });
            }
            return result;
        }

        public NodeViewModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (SetProperty(ref _selectedNode, value))
                {
                    // Update selected nodes collection
                    _selectedNodes.Clear();
                    if (value != null)
                    {
                        _selectedNodes.Add(value);
                    }
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public void UpdateSelectedNodes(IList<NodeViewModel> nodes)
        {
            _selectedNodes.Clear();
            foreach (var node in nodes)
            {
                _selectedNodes.Add(node);
            }
            _selectedNode = nodes.FirstOrDefault();
            CommandManager.InvalidateRequerySuggested();
        }

        public void UpdateSelectedNodes()
        {
            // 从所有节点中收集选中的节点
            _selectedNodes.Clear();
            CollectSelectedNodes(_rootNodes);
            _selectedNode = _selectedNodes.FirstOrDefault();
            CommandManager.InvalidateRequerySuggested();
        }

        private void CollectSelectedNodes(IEnumerable<NodeViewModel> nodes)
        {
            if (nodes == null)
                return;

            foreach (var node in nodes)
            {
                if (node != null)
                {
                    if (node.IsSelected)
                    {
                        _selectedNodes.Add(node);
                    }
                    CollectSelectedNodes(node.Children);
                }
            }
        }

        public ICommand OpenFileCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand OpenRecentCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExitCommand { get; }

        public ICommand CutNodeCommand { get; }
        public ICommand CopyNodeCommand { get; }
        public ICommand PasteNodeCommand { get; }
        public ICommand RenameNodeCommand { get; }
        public ICommand EditValueNodeCommand { get; }
        public ICommand DeleteNodeCommand { get; }
        public ICommand RefreshNodeCommand { get; }
        public ICommand CreateTagNodeCommand { get; }

        public ICommand FindCommand { get; }
        public ICommand FindNextCommand { get; }
        public ICommand FindBlockCommand { get; }
        public ICommand FindReplaceCommand { get; }
        public ICommand OpenMinecraftSaveFolderCommand { get; }
        public ICommand MoveUpNodeCommand { get; }
        public ICommand MoveDownNodeCommand { get; }
        public ICommand OpenInExplorerCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand AboutCommand { get; }

        private void OpenFile(object parameter)
        {
            if (!ConfirmAction("Open new file anyway?"))
                return;

            var dialog = new OpenFileDialog();
            dialog.Filter = "All NBT Files|*.mcr;*.mca;*.nbt;*.schematic;*.dat;level.dat|Region Files|*.mcr;*.mca|NBT Files|*.nbt;*.schematic;*.dat;level.dat|All Files|*.*";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                {
                    LoadFile(file);
                }
            }
        }

        private void OpenFolder(object parameter)
        {
            if (!ConfirmAction("Open new folder anyway?"))
                return;

            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadFolder(dialog.FolderName);
            }
        }

        private void OpenRecent(object parameter)
        {
            if (parameter is string path)
            {
                if (System.IO.Directory.Exists(path))
                {
                    LoadFolder(path);
                }
                else if (System.IO.File.Exists(path))
                {
                    LoadFile(path);
                }
                else
                {
                    // Handle missing file
                    System.Windows.MessageBox.Show($"File or directory not found: {path}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void Save(object parameter)
        {
            foreach (var node in _rootNodes)
            {
                node.Node.Save();
            }
        }

        private void Exit(object parameter)
        {
            if (ConfirmExit())
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void Find(object parameter)
        {
            var window = new FindWindow();
            window.Owner = System.Windows.Application.Current.MainWindow;
            if (window.ShowDialog() == true)
            {
                _searchState = new WpfSearchState(this)
                {
                    SearchName = window.MatchName ? window.NameToken : null,
                    SearchValue = window.MatchValue ? window.ValueToken : null,
                    RootNode = SelectedNode?.Node ?? _rootNodes.FirstOrDefault()?.Node
                };

                if (_searchState.RootNode == null) return;

                SearchNextNode();
            }
        }

        private void FindNext(object parameter)
        {
            if (_searchState != null)
            {
                SearchNextNode();
            }
            else
            {
                Find(parameter);
            }
        }

        private void SearchNextNode()
        {
            if (_searchState == null)
                return;

            _searchWorker = new SearchWorker(_searchState);
            var searchWindow = new CancelSearchWindow();
            searchWindow.Owner = System.Windows.Application.Current.MainWindow;

            // Run search in background thread
            System.Threading.Tasks.Task.Run(() =>
            {
                _searchWorker.Run();
                // Close search window when done
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (searchWindow.IsVisible)
                    {
                        searchWindow.Close();
                    }
                });
            });

            // Show search window and handle cancel
            if (searchWindow.ShowDialog() == true)
            {
                // User clicked cancel
                _searchWorker.Cancel();
                _searchState = null;
            }
        }

        private void FindReplace(object parameter)
        {
            var window = new FindReplaceWindow();
            window.Owner = System.Windows.Application.Current.MainWindow;
            var viewModel = window.DataContext as FindReplaceViewModel;
            if (viewModel != null)
            {
                viewModel.Initialize(this, SelectedNode?.Node ?? _rootNodes.FirstOrDefault()?.Node);
                window.Show();
            }
        }

        private void OpenMinecraftSaveFolder(object parameter)
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

                if (!System.IO.Directory.Exists(minecraftPath))
                {
                    minecraftPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                }

                LoadFolder(minecraftPath);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Could not open default Minecraft save directory", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                Console.WriteLine(e.Message);

                try
                {
                    LoadFolder(System.IO.Directory.GetCurrentDirectory());
                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Could not open current directory", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public void RefreshNode(DataNode node)
        {
            // Find the corresponding NodeViewModel and refresh it
            foreach (var rootNode in RootNodes)
            {
                var viewModel = FindNodeViewModel(rootNode, node);
                if (viewModel != null)
                {
                    viewModel.Refresh();
                    break;
                }
            }
        }

        private NodeViewModel FindNodeViewModel(NodeViewModel parentViewModel, DataNode targetNode)
        {
            if (parentViewModel.Node == targetNode)
            {
                return parentViewModel;
            }

            foreach (var childViewModel in parentViewModel.Children)
            {
                var result = FindNodeViewModel(childViewModel, targetNode);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void OpenSettings(object parameter)
        {
            var window = new SettingsWindow(_settingsService);
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
            OnPropertyChanged(nameof(RecentFiles)); // Refresh list in case it was cleared
        }

        private void About(object parameter)
        {
            var window = new AboutWindow();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void FindBlock(object parameter)
        {
            if (SelectedNode?.Node == null)
                return;

            var window = new FindBlockWindow(SelectedNode.Node);
            window.Owner = System.Windows.Application.Current.MainWindow;
            if (window.ShowDialog() == true)
            {
                if (window.Result != null)
                {
                    SelectNode(window.Result);
                }
                else
                {
                    System.Windows.MessageBox.Show("Chunk not found.", "Find Block", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
        }

        public void SelectNode(DataNode dataNode)
        {
            // Find the path from root to this node
            var path = new Stack<DataNode>();
            var curr = dataNode;
            while (curr != null)
            {
                path.Push(curr);
                curr = curr.Parent;
            }

            // Traverse ViewModels
            if (path.Count == 0) return;

            var rootDataNode = path.Pop();
            var rootViewModel = _rootNodes.FirstOrDefault(vm => vm.Node == rootDataNode);
            if (rootViewModel == null) return;

            var currentViewModel = rootViewModel;
            currentViewModel.IsExpanded = true;

            while (path.Count > 0)
            {
                var nextDataNode = path.Pop();
                var nextViewModel = currentViewModel.Children.FirstOrDefault(vm => vm.Node == nextDataNode);
                if (nextViewModel == null)
                {
                    // Maybe children not loaded yet? IsExpanded=true should trigger load.
                    // But load is async or sync? In NodeViewModel it is sync.
                    // However, ObservableCollection updates might need UI thread processing?
                    // No, we are on UI thread (InvokeDiscoverCallback uses Dispatcher).
                    
                    // Force refresh or wait? 
                    // If NodeViewModel lazily loads, it should be there after IsExpanded = true.
                    // Let's try to find it again.
                    break; 
                }
                currentViewModel = nextViewModel;
                currentViewModel.IsExpanded = true;
            }

            currentViewModel.IsSelected = true;
            // BringIntoView? ViewModel doesn't have reference to View.
            // We might need an event or attached behavior for BringIntoView.
        }

        public void LoadFile(string path)
        {
            var node = GetSupportedNode(path);
            if (node != null)
            {
                _rootNodes.Add(new NodeViewModel(node));
                AddToRecent(path);
            }
        }

        private DataNode GetSupportedNode(string path)
        {
            foreach (var record in FileTypeRegistry.RegisteredTypes)
            {
                if (record.Value.NamePatternTest(path))
                {
                    return record.Value.NodeCreate(path);
                }
            }
            return null;
        }

        public void LoadFolder(string path)
        {
            var node = new DirectoryDataNode(path);
            _rootNodes.Add(new NodeViewModel(node));
            AddToRecent(path);
        }

        private void AddToRecent(string path)
        {
            _settingsService.AddRecent(path);
            OnPropertyChanged(nameof(RecentFiles));
            OnPropertyChanged(nameof(RecentDirectories));
        }

        public List<string> RecentDirectories => _settingsService.Settings.RecentDirectories;

        #region Confirmation Methods

        public bool CheckModifications()
        {
            foreach (var rootNode in RootNodes)
            {
                if (rootNode.Node.IsModified)
                    return true;
            }
            return false;
        }

        public bool ConfirmAction(string actionMessage)
        {
            if (CheckModifications())
            {
                var result = System.Windows.MessageBox.Show(
                    $"You currently have unsaved changes. {actionMessage}",
                    "Unsaved Changes",
                    System.Windows.MessageBoxButton.OKCancel,
                    System.Windows.MessageBoxImage.Warning);
                return result == System.Windows.MessageBoxResult.OK;
            }
            return true;
        }

        public bool ConfirmExit()
        {
            if (CheckModifications())
            {
                var result = System.Windows.MessageBox.Show(
                    "You currently have unsaved changes. Close anyway?",
                    "Unsaved Changes",
                    System.Windows.MessageBoxButton.OKCancel,
                    System.Windows.MessageBoxImage.Warning);
                return result == System.Windows.MessageBoxResult.OK;
            }
            return true;
        }

        #endregion
    }
}
