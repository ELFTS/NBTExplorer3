using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NBTExplorer.Model;
using NBTExplorer.Model.Search;
using NBTModel.Interop;
using NBTExplorer.Wpf.Services;
using NBTExplorer.Wpf.Views;
using Substrate.Nbt;

namespace NBTExplorer.Wpf.ViewModels
{
    public class FindReplaceViewModel : ViewModelBase
    {
        private MainViewModel _mainViewModel;
        private DataNode _searchRoot;
        private SearchWorker _searchWorker;
        private ISearchState _searchState;
        private CancelSearchWindow _cancelSearchWindow;
        
        private RuleViewModel _selectedRule;
        private NodeViewModel _selectedReplaceNode;
        private bool _deleteExistingTags;
        private DataNode _currentFindNode;
        public DataNode CurrentFindNode { get { return _currentFindNode; } set { SetProperty(ref _currentFindNode, value); } }
        
        public FindReplaceViewModel()
        {
            FindRules = new ObservableCollection<RuleViewModel>();
            ReplaceTags = new ObservableCollection<NodeViewModel>();
            
            // Initialize fields
            _mainViewModel = null;
            _searchRoot = null;
            _searchWorker = null;
            _searchState = null;
            _cancelSearchWindow = null;
            _selectedRule = null;
            _selectedReplaceNode = null;
            _deleteExistingTags = false;
            _currentFindNode = null;
            
            // Initialize find rules with root
            var rootRule = new RootRule();
            FindRules.Add(new RuleViewModel(rootRule));
            
            // Initialize replace tags with virtual root
            var replaceRoot = new RootDataNode();
            replaceRoot.SetNodeName("ReplaceRoot");
            replaceRoot.SetDisplayName("Replacement Tags");
            ReplaceTags.Add(new NodeViewModel(replaceRoot));
            
            // Commands
            AddByteRuleCommand = new RelayCommand(AddByteRule);
            AddShortRuleCommand = new RelayCommand(AddShortRule);
            AddIntRuleCommand = new RelayCommand(AddIntRule);
            AddLongRuleCommand = new RelayCommand(AddLongRule);
            AddFloatRuleCommand = new RelayCommand(AddFloatRule);
            AddDoubleRuleCommand = new RelayCommand(AddDoubleRule);
            AddStringRuleCommand = new RelayCommand(AddStringRule);
            AddWildcardRuleCommand = new RelayCommand(AddWildcardRule);
            AddIntersectRuleCommand = new RelayCommand(AddIntersectRule);
            AddUnionRuleCommand = new RelayCommand(AddUnionRule);
            EditRuleCommand = new RelayCommand(EditRule, p => SelectedRule != null);
            DeleteRuleCommand = new RelayCommand(DeleteRule, p => SelectedRule != null && !(SelectedRule.Rule is RootRule));
            
            AddByteReplaceCommand = new RelayCommand(AddByteReplace);
            AddShortReplaceCommand = new RelayCommand(AddShortReplace);
            AddIntReplaceCommand = new RelayCommand(AddIntReplace);
            AddLongReplaceCommand = new RelayCommand(AddLongReplace);
            AddFloatReplaceCommand = new RelayCommand(AddFloatReplace);
            AddDoubleReplaceCommand = new RelayCommand(AddDoubleReplace);
            AddStringReplaceCommand = new RelayCommand(AddStringReplace);
            AddByteArrayReplaceCommand = new RelayCommand(AddByteArrayReplace);
            AddIntArrayReplaceCommand = new RelayCommand(AddIntArrayReplace);
            AddLongArrayReplaceCommand = new RelayCommand(AddLongArrayReplace);
            AddListReplaceCommand = new RelayCommand(AddListReplace);
            AddCompoundReplaceCommand = new RelayCommand(AddCompoundReplace);
            EditReplaceCommand = new RelayCommand(EditReplace, p => SelectedReplaceNode != null);
            DeleteReplaceCommand = new RelayCommand(DeleteReplace, p => SelectedReplaceNode != null && SelectedReplaceNode != ReplaceTags.First());
            
            FindCommand = new RelayCommand(Find);
            ReplaceCommand = new RelayCommand(Replace, p => _currentFindNode != null);
            ReplaceAllCommand = new RelayCommand(ReplaceAll);
            CancelCommand = new RelayCommand(Cancel);
        }
        
        public void Initialize(MainViewModel mainViewModel, DataNode searchRoot)
        {
            _mainViewModel = mainViewModel;
            _searchRoot = searchRoot;
        }
        
        public ObservableCollection<RuleViewModel> FindRules { get; }
        public ObservableCollection<NodeViewModel> ReplaceTags { get; }
        
        public RuleViewModel SelectedRule
        {
            get => _selectedRule;
            set => SetProperty(ref _selectedRule, value);
        }
        
        public NodeViewModel SelectedReplaceNode
        {
            get => _selectedReplaceNode;
            set => SetProperty(ref _selectedReplaceNode, value);
        }
        
        public bool DeleteExistingTags
        {
            get => _deleteExistingTags;
            set => SetProperty(ref _deleteExistingTags, value);
        }
        
        // Find Commands
        public ICommand AddByteRuleCommand { get; }
        public ICommand AddShortRuleCommand { get; }
        public ICommand AddIntRuleCommand { get; }
        public ICommand AddLongRuleCommand { get; }
        public ICommand AddFloatRuleCommand { get; }
        public ICommand AddDoubleRuleCommand { get; }
        public ICommand AddStringRuleCommand { get; }
        public ICommand AddWildcardRuleCommand { get; }
        public ICommand AddIntersectRuleCommand { get; }
        public ICommand AddUnionRuleCommand { get; }
        public ICommand EditRuleCommand { get; }
        public ICommand DeleteRuleCommand { get; }
        
        // Replace Commands
        public ICommand AddByteReplaceCommand { get; }
        public ICommand AddShortReplaceCommand { get; }
        public ICommand AddIntReplaceCommand { get; }
        public ICommand AddLongReplaceCommand { get; }
        public ICommand AddFloatReplaceCommand { get; }
        public ICommand AddDoubleReplaceCommand { get; }
        public ICommand AddStringReplaceCommand { get; }
        public ICommand AddByteArrayReplaceCommand { get; }
        public ICommand AddIntArrayReplaceCommand { get; }
        public ICommand AddLongArrayReplaceCommand { get; }
        public ICommand AddListReplaceCommand { get; }
        public ICommand AddCompoundReplaceCommand { get; }
        public ICommand EditReplaceCommand { get; }
        public ICommand DeleteReplaceCommand { get; }
        
        // Action Commands
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand ReplaceAllCommand { get; }
        public ICommand CancelCommand { get; }
        
        // Find Rule Methods
        private void AddByteRule(object parameter)
        {
            AddIntegralRule<ByteTagRule>(TagType.TAG_BYTE, "Byte");
        }
        
        private void AddShortRule(object parameter)
        {
            AddIntegralRule<ShortTagRule>(TagType.TAG_SHORT, "Short");
        }
        
        private void AddIntRule(object parameter)
        {
            AddIntegralRule<IntTagRule>(TagType.TAG_INT, "Int");
        }
        
        private void AddLongRule(object parameter)
        {
            AddIntegralRule<LongTagRule>(TagType.TAG_LONG, "Long");
        }
        
        private void AddFloatRule(object parameter)
        {
            AddFloatRule<FloatTagRule>(TagType.TAG_FLOAT, "Float");
        }
        
        private void AddDoubleRule(object parameter)
        {
            AddFloatRule<DoubleTagRule>(TagType.TAG_DOUBLE, "Double");
        }
        
        private void AddStringRule(object parameter)
        {
            var rule = new StringTagRule();
            var form = new StringRuleWindow(rule);
            if (form.ShowDialog() == true)
            {
                AddRuleToSelectedParent(rule);
            }
        }
        
        private void AddWildcardRule(object parameter)
        {
            var rule = new WildcardRule();
            var form = new WildcardRuleWindow(rule);
            if (form.ShowDialog() == true)
            {
                AddRuleToSelectedParent(rule);
            }
        }
        
        private void AddIntersectRule(object parameter)
        {
            AddRuleToSelectedParent(new IntersectRule());
        }
        
        private void AddUnionRule(object parameter)
        {
            AddRuleToSelectedParent(new UnionRule());
        }
        
        private void AddIntegralRule<T>(TagType tagType, string typeName) where T : SearchRule, new()
        {
            var rule = new T();
            var form = new ValueRuleWindow(SearchRule.NumericOpStrings.Values.ToList())
            {
                Title = $"Edit {typeName} Tag Rule"
            };
            
            if (form.ShowDialog() == true)
            {
                if (rule is IntegralTagRule<TagNodeByte> byteRule)
                {
                    byteRule.Name = form.TagName;
                    byteRule.Value = form.TagValueAsLong;
                    byteRule.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                else if (rule is IntegralTagRule<TagNodeShort> shortRule)
                {
                    shortRule.Name = form.TagName;
                    shortRule.Value = form.TagValueAsLong;
                    shortRule.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                else if (rule is IntegralTagRule<TagNodeInt> intRule)
                {
                    intRule.Name = form.TagName;
                    intRule.Value = form.TagValueAsLong;
                    intRule.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                else if (rule is IntegralTagRule<TagNodeLong> longRule)
                {
                    longRule.Name = form.TagName;
                    longRule.Value = form.TagValueAsLong;
                    longRule.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                AddRuleToSelectedParent(rule);
            }
        }
        
        private void AddFloatRule<T>(TagType tagType, string typeName) where T : SearchRule, new()
        {
            var rule = new T();
            var form = new ValueRuleWindow(SearchRule.NumericOpStrings.Values.ToList())
            {
                Title = $"Edit {typeName} Tag Rule"
            };
            
            if (form.ShowDialog() == true)
            {
                if (rule is FloatTagRule<TagNodeFloat> floatRule)
                {
                    floatRule.Name = form.TagName;
                    floatRule.Value = form.TagValueAsDouble;
                    floatRule.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                else if (rule is FloatTagRule<TagNodeDouble> doubleRule)
                {
                    doubleRule.Name = form.TagName;
                    doubleRule.Value = form.TagValueAsDouble;
                    doubleRule.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                AddRuleToSelectedParent(rule);
            }
        }
        
        private void AddRuleToSelectedParent(SearchRule rule)
        {
            var parentRule = SelectedRule ?? FindRules.First();
            if (parentRule.Rule is GroupRule groupRule)
            {
                groupRule.Rules.Add(rule);
                parentRule.Children.Add(new RuleViewModel(rule));
                parentRule.IsExpanded = true;
            }
        }
        
        private void EditRule(object parameter)
        {
            if (SelectedRule == null) return;
            
            var rule = SelectedRule.Rule;
            
            if (rule is ByteTagRule byteRule)
            {
                EditIntegralRule(byteRule, "Byte");
            }
            else if (rule is ShortTagRule shortRule)
            {
                EditIntegralRule(shortRule, "Short");
            }
            else if (rule is IntTagRule intRule)
            {
                EditIntegralRule(intRule, "Int");
            }
            else if (rule is LongTagRule longRule)
            {
                EditIntegralRule(longRule, "Long");
            }
            else if (rule is FloatTagRule floatRule)
            {
                EditFloatRule(floatRule, "Float");
            }
            else if (rule is DoubleTagRule doubleRule)
            {
                EditFloatRule(doubleRule, "Double");
            }
            else if (rule is StringTagRule stringRule)
            {
                var form = new StringRuleWindow(stringRule);
                if (form.ShowDialog() == true)
                {
                    SelectedRule.RefreshDisplay();
                }
            }
            else if (rule is WildcardRule wildcardRule)
            {
                var form = new WildcardRuleWindow(wildcardRule);
                if (form.ShowDialog() == true)
                {
                    SelectedRule.RefreshDisplay();
                }
            }
        }
        
        private void EditIntegralRule<T>(T rule, string typeName) where T : SearchRule
        {
            string tagName = "";
            string tagValue = "";
            string op = "";
            
            if (rule is IntegralTagRule<TagNodeByte> byteRule)
            {
                tagName = byteRule.Name;
                tagValue = byteRule.Value.ToString();
                op = SearchRule.NumericOpStrings[byteRule.Operator];
            }
            else if (rule is IntegralTagRule<TagNodeShort> shortRule)
            {
                tagName = shortRule.Name;
                tagValue = shortRule.Value.ToString();
                op = SearchRule.NumericOpStrings[shortRule.Operator];
            }
            else if (rule is IntegralTagRule<TagNodeInt> intRule)
            {
                tagName = intRule.Name;
                tagValue = intRule.Value.ToString();
                op = SearchRule.NumericOpStrings[intRule.Operator];
            }
            else if (rule is IntegralTagRule<TagNodeLong> longRule)
            {
                tagName = longRule.Name;
                tagValue = longRule.Value.ToString();
                op = SearchRule.NumericOpStrings[longRule.Operator];
            }
            
            var form = new ValueRuleWindow(SearchRule.NumericOpStrings.Values.ToList())
            {
                Title = $"Edit {typeName} Tag Rule",
                TagName = tagName,
                TagValue = tagValue,
                Operator = op
            };
            
            if (form.ShowDialog() == true)
            {
                if (rule is IntegralTagRule<TagNodeByte> byteRuleEdit)
                {
                    byteRuleEdit.Name = form.TagName;
                    byteRuleEdit.Value = form.TagValueAsLong;
                    byteRuleEdit.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                else if (rule is IntegralTagRule<TagNodeShort> shortRuleEdit)
                {
                    shortRuleEdit.Name = form.TagName;
                    shortRuleEdit.Value = form.TagValueAsLong;
                    shortRuleEdit.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                else if (rule is IntegralTagRule<TagNodeInt> intRuleEdit)
                {
                    intRuleEdit.Name = form.TagName;
                    intRuleEdit.Value = form.TagValueAsLong;
                    intRuleEdit.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                else if (rule is IntegralTagRule<TagNodeLong> longRuleEdit)
                {
                    longRuleEdit.Name = form.TagName;
                    longRuleEdit.Value = form.TagValueAsLong;
                    longRuleEdit.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                SelectedRule.RefreshDisplay();
            }
        }
        
        private void EditFloatRule<T>(T rule, string typeName) where T : SearchRule
        {
            string tagName = "";
            string tagValue = "";
            string op = "";
            
            if (rule is FloatTagRule<TagNodeFloat> floatRule)
            {
                tagName = floatRule.Name;
                tagValue = floatRule.Value.ToString();
                op = SearchRule.NumericOpStrings[floatRule.Operator];
            }
            else if (rule is FloatTagRule<TagNodeDouble> doubleRule)
            {
                tagName = doubleRule.Name;
                tagValue = doubleRule.Value.ToString();
                op = SearchRule.NumericOpStrings[doubleRule.Operator];
            }
            
            var form = new ValueRuleWindow(SearchRule.NumericOpStrings.Values.ToList())
            {
                Title = $"Edit {typeName} Tag Rule",
                TagName = tagName,
                TagValue = tagValue,
                Operator = op
            };
            
            if (form.ShowDialog() == true)
            {
                if (rule is FloatTagRule<TagNodeFloat> floatRuleEdit)
                {
                    floatRuleEdit.Name = form.TagName;
                    floatRuleEdit.Value = form.TagValueAsDouble;
                    floatRuleEdit.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                else if (rule is FloatTagRule<TagNodeDouble> doubleRuleEdit)
                {
                    doubleRuleEdit.Name = form.TagName;
                    doubleRuleEdit.Value = form.TagValueAsDouble;
                    doubleRuleEdit.Operator = (NumericOperator)SearchRule.NumericOpStrings.First(x => x.Value == form.Operator).Key;
                }
                SelectedRule.RefreshDisplay();
            }
        }
        
        private void DeleteRule(object parameter)
        {
            if (SelectedRule == null || SelectedRule.Rule is RootRule) return;
            
            // Find parent rule
            var parentRule = FindParentRule(SelectedRule);
            if (parentRule == null) return;
            
            // Remove from parent
            if (parentRule.Rule is GroupRule groupRule)
            {
                groupRule.Rules.Remove(SelectedRule.Rule);
                parentRule.Children.Remove(SelectedRule);
            }
        }
        
        private RuleViewModel FindParentRule(RuleViewModel childRule)
        {
            foreach (var rule in FindRules)
            {
                if (rule.Children.Contains(childRule))
                {
                    return rule;
                }
                
                var parent = FindParentRuleRecursive(rule, childRule);
                if (parent != null)
                {
                    return parent;
                }
            }
            
            return null;
        }
        
        private RuleViewModel FindParentRuleRecursive(RuleViewModel parentRule, RuleViewModel childRule)
        {
            foreach (var rule in parentRule.Children)
            {
                if (rule.Children.Contains(childRule))
                {
                    return rule;
                }
                
                var parent = FindParentRuleRecursive(rule, childRule);
                if (parent != null)
                {
                    return parent;
                }
            }
            
            return null;
        }
        
        // Replace Tag Methods
        private void AddByteReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_BYTE);
        }
        
        private void AddShortReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_SHORT);
        }
        
        private void AddIntReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_INT);
        }
        
        private void AddLongReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_LONG);
        }
        
        private void AddFloatReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_FLOAT);
        }
        
        private void AddDoubleReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_DOUBLE);
        }
        
        private void AddStringReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_STRING);
        }
        
        private void AddByteArrayReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_BYTE_ARRAY);
        }
        
        private void AddIntArrayReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_INT_ARRAY);
        }
        
        private void AddLongArrayReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_LONG_ARRAY);
        }
        
        private void AddListReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_LIST);
        }
        
        private void AddCompoundReplace(object parameter)
        {
            AddReplaceTag(TagType.TAG_COMPOUND);
        }
        
        private void AddReplaceTag(TagType tagType)
        {
            var replaceRoot = ReplaceTags.First().Node as RootDataNode;
            if (replaceRoot == null) return;
            
            var formData = new CreateTagFormData
            {
                TagType = tagType,
                HasName = true
            };
            
            // Add existing tag names to restricted names to avoid duplicates
            foreach (var child in replaceRoot.Nodes)
            {
                formData.RestrictedNames.Add(child.NodeName);
            }
            
            var form = new CreateNodeWindow(formData);
            if (form.ShowDialog() == true)
            {
                var tag = formData.TagNode;
                var tagName = formData.TagName;
                
                if (tag != null && !string.IsNullOrEmpty(tagName))
                {
                    replaceRoot.NamedTagContainer.AddTag(tag, tagName);
                    replaceRoot.SyncTag();
                    
                    // Refresh replace tags tree
                    RefreshReplaceTags();
                }
            }
        }
        
        private void EditReplace(object parameter)
        {
            if (SelectedReplaceNode != null && SelectedReplaceNode.Node is TagDataNode tagNode)
            {
                tagNode.EditNode();
                RefreshReplaceTags();
            }
        }
        
        private void DeleteReplace(object parameter)
        {
            if (SelectedReplaceNode != null && SelectedReplaceNode.Node is TagDataNode tagNode)
            {
                tagNode.DeleteNode();
                RefreshReplaceTags();
            }
        }
        
        private void RefreshReplaceTags()
        {
            var replaceRoot = ReplaceTags.FirstOrDefault();
            if (replaceRoot != null)
            {
                replaceRoot.Children.Clear();
                
                if (replaceRoot.Node is TagCompoundDataNode compoundNode)
                {
                    foreach (var child in compoundNode.Nodes)
                    {
                        replaceRoot.Children.Add(new NodeViewModel(child));
                    }
                }
            }
        }
        
        // Search and Replace Methods
        private void Find(object parameter)
        {
            if (_mainViewModel == null) return;
            
            var rootRule = FindRules.First().Rule as RootRule;
            if (rootRule == null) return;
            
            _searchState = new ContainerRuleSearchState(_mainViewModel, rootRule)
            {
                RootNode = _searchRoot ?? _mainViewModel.RootNodes.FirstOrDefault()?.Node
            };
            
            if (_searchState.RootNode == null) return;
            
            _searchWorker = new SearchWorker(_searchState);
            
            // Show cancel search window
            _cancelSearchWindow = new CancelSearchWindow();
            _cancelSearchWindow.Owner = Application.Current.MainWindow;
            
            // Run search in background
            Task.Run(() => _searchWorker.Run());
            
            // Show cancel window
            if (_cancelSearchWindow.ShowDialog() == false)
            {
                // User cancelled search
                _searchWorker.Cancel();
                _searchState = null;
            }
        }
        
        private void Replace(object parameter)
        {
            if (CurrentFindNode == null) return;
            
            ReplaceCurrent();
        }
        
        private void ReplaceAll(object parameter)
        {
            if (_mainViewModel == null) return;
            
            var rootRule = FindRules.First().Rule as RootRule;
            if (rootRule == null) return;
            
            _searchState = new ContainerRuleSearchState(_mainViewModel, rootRule)
            {
                RootNode = _searchRoot ?? _mainViewModel.RootNodes.FirstOrDefault()?.Node,
                TerminateOnDiscover = false,
                ReplaceAll = true
            };
            
            if (_searchState.RootNode == null) return;
            
            _searchWorker = new SearchWorker(_searchState);
            
            // Show cancel search window
            _cancelSearchWindow = new CancelSearchWindow();
            _cancelSearchWindow.Owner = Application.Current.MainWindow;
            
            // Run search in background
            Task.Run(() => _searchWorker.Run());
            
            // Show cancel window
            if (_cancelSearchWindow.ShowDialog() == false)
            {
                // User cancelled search
                _searchWorker.Cancel();
                _searchState = null;
            }
        }
        
        private void Cancel(object parameter)
        {
            Close();
        }
        
        private void Close()
        {
            var window = Application.Current.Windows.OfType<FindReplaceWindow>().FirstOrDefault();
            window?.Close();
        }
        
        // Search Callbacks
        private void SearchDiscoveryCallback(DataNode node)
        {
            _mainViewModel.SelectNode(node);
            _currentFindNode = node;
            
            if (_cancelSearchWindow != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _cancelSearchWindow.DialogResult = true;
                    _cancelSearchWindow = null;
                });
            }
        }
        
        private void SearchDiscoveryReplaceAllCallback(DataNode node)
        {
            _mainViewModel.SelectNode(node);
            _currentFindNode = node;
            
            ReplaceCurrent();
        }
        
        private void SearchCollapseCallback(DataNode node)
        {
            // Collapse node in UI
        }
        
        private void SearchProgressCallback(DataNode node)
        {
            if (_cancelSearchWindow != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _cancelSearchWindow.SearchPath = node.NodePath;
                });
            }
        }
        
        private void SearchEndCallback(DataNode node)
        {
            if (_cancelSearchWindow != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _cancelSearchWindow.DialogResult = true;
                    _cancelSearchWindow = null;
                });
            }
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("End of Results", "Search Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
        
        public void ReplaceCurrent()
        {
            var node = CurrentFindNode as TagCompoundDataNode;
            if (node == null) return;
            
            var rootRule = FindRules.First().Rule as RootRule;
            if (rootRule == null) return;
            
            var replaceRoot = ReplaceTags.First().Node as TagCompoundDataNode;
            if (replaceRoot == null) return;
            
            // Find matching nodes
            var matches = new List<TagDataNode>();
            rootRule.Matches(node, matches);
            
            // Get replace tag names
            var replaceNames = new List<string>();
            foreach (var rnode in replaceRoot.Nodes)
            {
                replaceNames.Add(rnode.NodeName);
            }
            
            // Delete matching or existing tags
            foreach (var match in matches)
            {
                if (DeleteExistingTags || replaceNames.Contains(match.NodeName))
                {
                    match.DeleteNode();
                }
            }
            
            // Add replacement tags
            foreach (var tagNode in replaceRoot.Nodes)
            {
                if (tagNode is TagDataNode tagDataNode)
                {
                    node.NamedTagContainer.AddTag(tagDataNode.Tag, tagDataNode.NodeName);
                    node.SyncTag();
                }
            }
            
            // Refresh UI
            _mainViewModel.RefreshNode(node);
        }
    }
    
    public class RuleViewModel : ViewModelBase
    {
        private SearchRule _rule;
        private ObservableCollection<RuleViewModel> _children;
        private bool _isExpanded;
        private string _display;
        
        public RuleViewModel(SearchRule rule)
        {
            _rule = rule;
            _children = new ObservableCollection<RuleViewModel>();
            _display = rule.NodeDisplay;
            
            // Initialize children for group rules
            if (rule is GroupRule groupRule)
            {
                foreach (var childRule in groupRule.Rules)
                {
                    _children.Add(new RuleViewModel(childRule));
                }
            }
        }
        
        public SearchRule Rule => _rule;
        public ObservableCollection<RuleViewModel> Children => _children;
        
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
        
        public string Display
        {
            get => _display;
            set => SetProperty(ref _display, value);
        }
        
        public void RefreshDisplay()
        {
            Display = _rule.NodeDisplay;
        }
    }
}
