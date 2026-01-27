using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using NBTExplorer.Model;
using Substrate.Nbt;

namespace NBTExplorer.Wpf.ViewModels
{
    public class NodeViewModel : ViewModelBase
    {
        private readonly DataNode _node;
        private readonly NodeViewModel _parent;
        private ObservableCollection<NodeViewModel> _children;
        private bool _isExpanded;
        private bool _isSelected;

        public NodeViewModel(DataNode node, NodeViewModel parent = null)
        {
            _node = node ?? throw new ArgumentNullException(nameof(node));
            _parent = parent;
            _children = new ObservableCollection<NodeViewModel>();
            
            RenameCommand = new RelayCommand(p => Rename(), p => _node.CanRenameNode);
            EditValueCommand = new RelayCommand(p => EditValue(), p => _node.CanEditNode);
            DeleteCommand = new RelayCommand(p => Delete(), p => _node.CanDeleteNode);
            CutCommand = new RelayCommand(p => Cut(), p => _node.CanCutNode);
            CopyCommand = new RelayCommand(p => Copy(), p => _node.CanCopyNode);
            PasteCommand = new RelayCommand(p => Paste(), p => _node.CanPasteIntoNode);
            RefreshCommand = new RelayCommand(p => RefreshNode(), p => _node.CanRefreshNode);
            CreateTagCommand = new RelayCommand(p => CreateTag((TagType)p), p => CanCreateTag((TagType)p));
            MoveUpCommand = new RelayCommand(p => MoveUp(), p => _node.CanMoveNodeUp);
            MoveDownCommand = new RelayCommand(p => MoveDown(), p => _node.CanMoveNodeDown);
            OpenInExplorerCommand = new RelayCommand(p => OpenInExplorer(), p => _node is DirectoryDataNode);

            if (_node.IsExpanded)
            {
                LoadChildren();
                _isExpanded = true;
            }
            else if (_node.HasUnexpandedChildren)
            {
                _children.Add(null); // Dummy node for lazy loading
            }
        }

        public DataNode Node => _node;
        public NodeViewModel Parent => _parent;

        public string Name => _node.NodePathName;
        public string Display => _node.NodeDisplay;

        public ICommand RenameCommand { get; }
        public ICommand EditValueCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateTagCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand OpenInExplorerCommand { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value))
                {
                    if (_isExpanded)
                    {
                        _node.Expand();
                        LoadChildren();
                    }
                    else
                    {
                        _node.Collapse();
                    }
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    // 通知MainViewModel更新选中节点列表
                    var mainViewModel = FindMainViewModel(this);
                    mainViewModel?.UpdateSelectedNodes();
                }
            }
        }

        private MainViewModel FindMainViewModel(NodeViewModel node)
        {
            if (node.Parent == null)
            {
                // 检查是否是MainViewModel的根节点
                // 这里我们需要一种方式来获取MainViewModel的引用
                // 由于没有直接引用，我们可以通过应用程序的主窗口来获取
                var mainWindow = System.Windows.Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    return mainWindow.DataContext as MainViewModel;
                }
                return null;
            }
            return FindMainViewModel(node.Parent);
        }

        public ObservableCollection<NodeViewModel> Children => _children;

        private void LoadChildren()
        {
            // Remove any dummy nodes (null entries)
            _children.Remove(null);
            
            // Create a dictionary of existing children for quick lookup
            var existingChildren = _children.ToDictionary(vm => vm.Node, vm => vm);
            var newNodes = _node.Nodes.ToList();
            
            // Remove children that no longer exist
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                var childVm = _children[i];
                if (!newNodes.Contains(childVm.Node))
                {
                    _children.RemoveAt(i);
                }
            }
            
            // Add or update children
            for (int i = 0; i < newNodes.Count; i++)
            {
                var childNode = newNodes[i];
                if (existingChildren.TryGetValue(childNode, out var existingVm))
                {
                    // Update existing node
                    existingVm.Refresh();
                    // Ensure node is in the correct position
                    if (_children.IndexOf(existingVm) != i)
                    {
                        _children.Remove(existingVm);
                        _children.Insert(i, existingVm);
                    }
                }
                else
                {
                    // Add new node
                    _children.Insert(i, new NodeViewModel(childNode, this));
                }
            }
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Display));
            if (_isExpanded)
            {
                LoadChildren();
            }
        }

        private void Rename()
        {
            if (_node.RenameNode())
            {
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Display));
            }
        }

        private void EditValue()
        {
            if (_node.EditNode())
            {
                OnPropertyChanged(nameof(Display));
            }
        }

        private void Delete()
        {
            if (_node.DeleteNode())
            {
                if (_parent != null)
                {
                    _parent.Children.Remove(this);
                }
                // If parent is null (root), it should be handled by MainViewModel
            }
        }

        private void Cut()
        {
            if (_node.CutNode())
            {
                 if (_parent != null)
                {
                    _parent.Children.Remove(this);
                }
            }
        }

        private void Copy()
        {
            _node.CopyNode();
        }

        private void Paste()
        {
            if (_node.PasteNode())
            {
                // We need to refresh children because a new node was added
                if (_isExpanded)
                {
                    LoadChildren();
                }
                else
                {
                    // If not expanded, maybe just ensure the dummy node is there or expand it
                    IsExpanded = true; 
                }
            }
        }

        private void RefreshNode()
        {
            if (_node.RefreshNode())
            {
                LoadChildren();
            }
        }

        private bool CanCreateTag(TagType type)
        {
            return _node.CanCreateTag(type);
        }

        private void CreateTag(TagType type)
        {
            if (_node.CreateNode(type))
            {
                if (!_isExpanded)
                {
                    IsExpanded = true;
                }
                else
                {
                    LoadChildren();
                }
            }
        }

        private void MoveUp()
        {
            if (_node.ChangeRelativePosition(-1))
            {
                if (_parent != null)
                {
                    // Find current index
                    int currentIndex = _parent.Children.IndexOf(this);
                    if (currentIndex > 0)
                    {
                        // Move in the collection
                        _parent.Children.RemoveAt(currentIndex);
                        _parent.Children.Insert(currentIndex - 1, this);
                    }
                }
            }
        }

        private void MoveDown()
        {
            if (_node.ChangeRelativePosition(1))
            {
                if (_parent != null)
                {
                    // Find current index
                    int currentIndex = _parent.Children.IndexOf(this);
                    if (currentIndex < _parent.Children.Count - 1)
                    {
                        // Move in the collection
                        _parent.Children.RemoveAt(currentIndex);
                        _parent.Children.Insert(currentIndex + 1, this);
                    }
                }
            }
        }

        private void OpenInExplorer()
        {
            if (_node is DirectoryDataNode directoryNode)
            {
                try
                {
                    string path = directoryNode.NodeDirPath;
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Can't open directory: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public string Icon
        {
            get
            {
                if (_node is TagDataNode tagNode)
                {
                    return GetIconForTagType(tagNode.Tag.GetTagType());
                }
                if (_node is RegionChunkDataNode) return "/Resources/block.png";
                if (_node is RegionFileDataNode) return "/Resources/file.ico";
                if (_node is DirectoryDataNode) return "/Resources/folder.png";
                if (_node is CubicRegionDataNode) return "/Resources/block.png";
                
                return "/Resources/document-24.png";
            }
        }

        private string GetIconForTagType(TagType type)
        {
            switch (type)
            {
                case TagType.TAG_BYTE: return "/Resources/document-attribute-b.png";
                case TagType.TAG_SHORT: return "/Resources/document-attribute-s.png";
                case TagType.TAG_INT: return "/Resources/document-attribute-i.png";
                case TagType.TAG_LONG: return "/Resources/document-attribute-l.png";
                case TagType.TAG_FLOAT: return "/Resources/document-attribute-f.png";
                case TagType.TAG_DOUBLE: return "/Resources/document-attribute-d.png";
                case TagType.TAG_BYTE_ARRAY: return "/Resources/edit-code-b.png";
                case TagType.TAG_STRING: return "/Resources/edit-small-caps.png";
                case TagType.TAG_LIST: return "/Resources/edit-list.png";
                case TagType.TAG_COMPOUND: return "/Resources/box.ico";
                case TagType.TAG_INT_ARRAY: return "/Resources/edit-code-i.png";
                case TagType.TAG_SHORT_ARRAY: return "/Resources/edit-code-s.png";
                case TagType.TAG_LONG_ARRAY: return "/Resources/edit-code-l.png";
                default: return "/Resources/document-24.png";
            }
        }
    }
}
