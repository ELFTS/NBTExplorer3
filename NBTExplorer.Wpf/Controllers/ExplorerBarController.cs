using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NBTExplorer.Model;
using NBTExplorer.Wpf.Services;

namespace NBTExplorer.Wpf.Controllers
{
    public class ExplorerBarController
    {
        private ToolBar _explorerBar;
        private DataNode _rootNode;
        private IconRegistry _iconRegistry;

        public ExplorerBarController(ToolBar explorerBar, IconRegistry iconRegistry, DataNode rootNode)
        {
            _explorerBar = explorerBar;
            _iconRegistry = iconRegistry;
            _rootNode = rootNode;

            Initialize();
        }

        private void Initialize()
        {
            _explorerBar.Items.Clear();

            List<DataNode> ancestry = new List<DataNode>();
            DataNode node = _rootNode;

            while (node != null)
            {
                ancestry.Add(node);
                node = node.Parent;
            }

            ancestry.Reverse();

            foreach (DataNode item in ancestry)
            {
                MenuItem itemButton = new MenuItem
                {
                    Header = item.NodePathName,
                    Tag = item,
                };
                itemButton.Click += (s, e) =>
                {
                    MenuItem menuItem = s as MenuItem;
                    if (menuItem != null)
                        SearchRoot = menuItem.Tag as DataNode;
                };

                if (!item.IsExpanded)
                    item.Expand();

                foreach (DataNode subItem in item.Nodes)
                {
                    if (!subItem.IsContainerType)
                        continue;

                    MenuItem subMenuItem = new MenuItem
                    {
                        Header = subItem.NodePathName,
                        Tag = subItem,
                    };

                    subMenuItem.Click += (s, e) =>
                    {
                        MenuItem menuItem = s as MenuItem;
                        if (menuItem != null)
                            SearchRoot = menuItem.Tag as DataNode;
                    };

                    if (ancestry.Contains(subItem))
                    {
                        // 在WPF中，我们可以通过样式来设置粗体
                        var style = new System.Windows.Style(typeof(MenuItem));
                        style.Setters.Add(new System.Windows.Setter(System.Windows.Controls.Control.FontWeightProperty, System.Windows.FontWeights.Bold));
                        subMenuItem.Style = style;
                    }

                    itemButton.Items.Add(subMenuItem);
                }

                _explorerBar.Items.Add(itemButton);
            }
        }

        public DataNode SearchRoot
        {
            get { return _rootNode; }
            set
            {
                if (_rootNode == value)
                    return;

                _rootNode = value;
                Initialize();

                OnSearchRootChanged();
            }
        }

        public event EventHandler SearchRootChanged;

        protected virtual void OnSearchRootChanged()
        {
            SearchRootChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
