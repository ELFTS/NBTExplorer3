using System.Windows.Controls;
using NBTExplorer.Model.Search;
using NBTExplorer.Wpf.Services;
using NBTExplorer.Wpf.Views;
using Substrate.Nbt;

namespace NBTExplorer.Wpf.Controllers
{
    public class RuleTreeController
    {
        private TreeView _nodeTree;
        private IconRegistry _iconRegistry;

        private RootRule _rootData;

        public RuleTreeController(TreeView nodeTree)
        {
            _nodeTree = nodeTree;

            InitializeIconRegistry();
            ShowVirtualRoot = true;

            _rootData = new RootRule();

            RefreshTree();
        }

        private void InitializeIconRegistry()
        {
            _iconRegistry = new IconRegistry();
            _iconRegistry.DefaultIcon = 15;

            _iconRegistry.Register(typeof(RootRule), 18);
            _iconRegistry.Register(typeof(UnionRule), 21);
            _iconRegistry.Register(typeof(IntersectRule), 20);
            _iconRegistry.Register(typeof(WildcardRule), 19);
            _iconRegistry.Register(typeof(ByteTagRule), 0);
            _iconRegistry.Register(typeof(ShortTagRule), 1);
            _iconRegistry.Register(typeof(IntTagRule), 2);
            _iconRegistry.Register(typeof(LongTagRule), 3);
            _iconRegistry.Register(typeof(FloatTagRule), 4);
            _iconRegistry.Register(typeof(DoubleTagRule), 5);
            _iconRegistry.Register(typeof(StringTagRule), 7);
        }

        public RootRule Root
        {
            get { return _rootData; }
        }

        public TreeView Tree
        {
            get { return _nodeTree; }
        }

        public bool ShowVirtualRoot { get; set; }

        public string VirtualRootDisplay
        {
            get { return _rootData.NodeDisplay; }
        }

        public void DeleteSelection()
        {
            DeleteNode(SelectedNode);
        }

        public void DeleteNode(TreeViewItem node)
        {
            if (node == null || !(node.Tag is SearchRule))
                return;

            TreeViewItem parent = node.Parent as TreeViewItem;
            if (parent == null || !(parent.Tag is GroupRule))
                return;

            GroupRule parentData = parent.Tag as GroupRule;
            SearchRule nodeData = node.Tag as SearchRule;

            parentData.Rules.Remove(nodeData);
            parent.Items.Remove(node);
        }

        public TreeViewItem SelectedNode
        {
            get { return _nodeTree.SelectedItem as TreeViewItem; }
        }

        public TreeViewItem SelectedOrRootNode
        {
            get { return SelectedNode ?? (_nodeTree.Items.Count > 0 ? _nodeTree.Items[0] as TreeViewItem : null); }
        }

        private TreeViewItem CreateIntegralNode<T, K>(string typeName)
            where K : TagNode
            where T : IntegralTagRule<K>, new()
        {
            T rule = new T();

            // 创建一个空的操作符列表，因为我们只是创建一个新规则
            var operators = new System.Collections.Generic.List<string>();
            ValueRuleWindow window = new ValueRuleWindow(operators)
            {
                Title = "Edit " + typeName + " Tag Rule",
            };

            if (window.ShowDialog() == true)
            {
                rule.Name = window.TagName;
                rule.Value = window.TagValueAsLong;
                // 这里需要根据实际情况设置operator
            }
            else
                return null;

            TreeViewItem node = CreateNode(rule);
            node.Header = rule.NodeDisplay;

            return node;
        }

        private void EditIntegralNode<T, K>(TreeViewItem node, T rule, string typeName)
            where K : TagNode
            where T : IntegralTagRule<K>
        {
            // 创建一个空的操作符列表，因为我们只是编辑一个现有规则
            var operators = new System.Collections.Generic.List<string>();
            ValueRuleWindow window = new ValueRuleWindow(operators)
            {
                Title = "Edit " + typeName + " Tag Rule",
                TagName = rule.Name,
                TagValue = rule.Value.ToString(),
            };

            if (window.ShowDialog() == true)
            {
                rule.Name = window.TagName;
                rule.Value = window.TagValueAsLong;
                // 这里需要根据实际情况设置operator
            }

            node.Header = rule.NodeDisplay;
        }

        private TreeViewItem CreateFloatNode<T, K>(string typeName)
            where K : TagNode
            where T : FloatTagRule<K>, new()
        {
            T rule = new T();

            // 创建一个空的操作符列表，因为我们只是创建一个新规则
            var operators = new System.Collections.Generic.List<string>();
            ValueRuleWindow window = new ValueRuleWindow(operators)
            {
                Title = "Edit " + typeName + " Tag Rule",
            };

            if (window.ShowDialog() == true)
            {
                rule.Name = window.TagName;
                rule.Value = window.TagValueAsDouble;
                // 这里需要根据实际情况设置operator
            }
            else
                return null;

            TreeViewItem node = CreateNode(rule);
            node.Header = rule.NodeDisplay;

            return node;
        }

        private void EditFloatNode<T, K>(TreeViewItem node, T rule, string typeName)
            where K : TagNode
            where T : FloatTagRule<K>
        {
            // 创建一个空的操作符列表，因为我们只是编辑一个现有规则
            var operators = new System.Collections.Generic.List<string>();
            ValueRuleWindow window = new ValueRuleWindow(operators)
            {
                Title = "Edit " + typeName + " Tag Rule",
                TagName = rule.Name,
                TagValue = rule.Value.ToString(),
            };

            if (window.ShowDialog() == true)
            {
                rule.Name = window.TagName;
                rule.Value = window.TagValueAsDouble;
                // 这里需要根据实际情况设置operator
            }

            node.Header = rule.NodeDisplay;
        }

        private TreeViewItem CreateStringNode(string typeName)
        {
            StringTagRule rule = new StringTagRule();

            StringRuleWindow window = new StringRuleWindow(rule)
            {
                Title = "Edit " + typeName + " Tag Rule",
            };

            if (window.ShowDialog() == true)
            {
                // 规则已经在StringRuleWindow中更新
            }
            else
                return null;

            TreeViewItem node = CreateNode(rule);
            node.Header = rule.NodeDisplay;

            return node;
        }

        private void EditStringNode(TreeViewItem node, StringTagRule rule, string typeName)
        {
            StringRuleWindow window = new StringRuleWindow(rule)
            {
                Title = "Edit " + typeName + " Tag Rule",
            };

            if (window.ShowDialog() == true)
            {
                // 规则已经在StringRuleWindow中更新
            }

            node.Header = rule.NodeDisplay;
        }

        private TreeViewItem CreateWildcardNode(string typeName)
        {
            WildcardRule rule = new WildcardRule();

            WildcardRuleWindow window = new WildcardRuleWindow(rule)
            {
                Title = "Edit " + typeName + " Rule",
            };

            if (window.ShowDialog() == true)
            {
                // 规则已经在WildcardRuleWindow中更新
            }
            else
                return null;

            TreeViewItem node = CreateNode(rule);
            node.Header = rule.NodeDisplay;

            return node;
        }

        private void EditWildcardNode(TreeViewItem node, WildcardRule rule, string typeName)
        {
            WildcardRuleWindow window = new WildcardRuleWindow(rule)
            {
                Title = "Edit " + typeName + " Rule",
            };

            if (window.ShowDialog() == true)
            {
                // 规则已经在WildcardRuleWindow中更新
            }

            node.Header = rule.NodeDisplay;
        }

        public void CreateNode(TreeViewItem node, TagType type)
        {
            if (node == null || !(node.Tag is GroupRule))
                return;

            GroupRule dataNode = node.Tag as GroupRule;
            TreeViewItem newNode = null;

            switch (type)
            {
                case TagType.TAG_BYTE:
                    newNode = CreateIntegralNode<ByteTagRule, TagNodeByte>("Byte");
                    break;
                case TagType.TAG_SHORT:
                    newNode = CreateIntegralNode<ShortTagRule, TagNodeShort>("Short");
                    break;
                case TagType.TAG_INT:
                    newNode = CreateIntegralNode<IntTagRule, TagNodeInt>("Int");
                    break;
                case TagType.TAG_LONG:
                    newNode = CreateIntegralNode<LongTagRule, TagNodeLong>("Long");
                    break;
                case TagType.TAG_FLOAT:
                    newNode = CreateFloatNode<FloatTagRule, TagNodeFloat>("Float");
                    break;
                case TagType.TAG_DOUBLE:
                    newNode = CreateFloatNode<DoubleTagRule, TagNodeDouble>("Double");
                    break;
                case TagType.TAG_STRING:
                    newNode = CreateStringNode("String");
                    break;
            }

            if (newNode != null)
            {
                node.Items.Add(newNode);
                dataNode.Rules.Add(newNode.Tag as SearchRule);

                node.IsExpanded = true;
            }
        }

        public void EditNode(TreeViewItem node)
        {
            if (node == null || !(node.Tag is SearchRule))
                return;

            SearchRule rule = node.Tag as SearchRule;

            if (rule is ByteTagRule)
                EditIntegralNode<ByteTagRule, TagNodeByte>(node, rule as ByteTagRule, "Byte");
            else if (rule is ShortTagRule)
                EditIntegralNode<ShortTagRule, TagNodeShort>(node, rule as ShortTagRule, "Short");
            else if (rule is IntTagRule)
                EditIntegralNode<IntTagRule, TagNodeInt>(node, rule as IntTagRule, "Int");
            else if (rule is LongTagRule)
                EditIntegralNode<LongTagRule, TagNodeLong>(node, rule as LongTagRule, "Long");
            else if (rule is FloatTagRule)
                EditFloatNode<FloatTagRule, TagNodeFloat>(node, rule as FloatTagRule, "Float");
            else if (rule is DoubleTagRule)
                EditFloatNode<DoubleTagRule, TagNodeDouble>(node, rule as DoubleTagRule, "Double");
            else if (rule is StringTagRule)
                EditStringNode(node, rule as StringTagRule, "String");
            else if (rule is WildcardRule)
                EditWildcardNode(node, rule as WildcardRule, "Wildcard");
        }

        public void EditSelection()
        {
            if (SelectedNode == null)
                return;

            EditNode(SelectedNode);
        }

        public void CreateWildcardNode(TreeViewItem node)
        {
            if (node == null || !(node.Tag is GroupRule))
                return;

            GroupRule dataNode = node.Tag as GroupRule;

            TreeViewItem newNode = CreateWildcardNode("Wildcard");

            if (newNode != null)
            {
                node.Items.Add(newNode);
                dataNode.Rules.Add(newNode.Tag as SearchRule);

                node.IsExpanded = true;
            }
        }

        public void CreateWildcardNode()
        {
            CreateWildcardNode(SelectedOrRootNode);
        }

        public void CreateUnionNode(TreeViewItem node)
        {
            if (node == null || !(node.Tag is GroupRule))
                return;

            GroupRule dataNode = node.Tag as GroupRule;

            TreeViewItem newNode = CreateNode(new UnionRule());
            node.Items.Add(newNode);
            dataNode.Rules.Add(newNode.Tag as SearchRule);

            node.IsExpanded = true;
        }

        public void CreateUnionNode()
        {
            CreateUnionNode(SelectedOrRootNode);
        }

        public void CreateIntersectNode(TreeViewItem node)
        {
            if (node == null || !(node.Tag is GroupRule))
                return;

            GroupRule dataNode = node.Tag as GroupRule;

            TreeViewItem newNode = CreateNode(new IntersectRule());
            node.Items.Add(newNode);
            dataNode.Rules.Add(newNode.Tag as SearchRule);

            node.IsExpanded = true;
        }

        public void CreateIntersectNode()
        {
            CreateIntersectNode(SelectedOrRootNode);
        }

        public void CreateNode(TagType type)
        {
            if (SelectedOrRootNode == null)
                return;

            CreateNode(SelectedOrRootNode, type);
        }

        private TreeViewItem CreateNode(SearchRule rule)
        {
            TreeViewItem treeNode = new TreeViewItem
            {
                Header = rule.NodeDisplay,
                Tag = rule,
            };

            return treeNode;
        }

        private void ExpandNode(TreeViewItem node, bool recurse)
        {
            GroupRule rule = node.Tag as GroupRule;
            if (rule == null)
                return;

            foreach (var subRule in rule.Rules)
            {
                TreeViewItem subNode = CreateNode(subRule);
                node.Items.Add(subNode);

                if (recurse)
                    ExpandNode(subNode, recurse);
            }
        }

        private void RefreshTree()
        {
            _nodeTree.Items.Clear();
            var rootNode = CreateNode(_rootData);
            _nodeTree.Items.Add(rootNode);

            ExpandNode(rootNode, true);

            // WPF的TreeView没有ExpandAll()方法，我们需要递归展开所有节点
            ExpandAllNodes(rootNode);
        }

        private void ExpandAllNodes(TreeViewItem node)
        {
            node.IsExpanded = true;
            foreach (var item in node.Items)
            {
                var childNode = item as TreeViewItem;
                if (childNode != null)
                {
                    ExpandAllNodes(childNode);
                }
            }
        }
    }
}
