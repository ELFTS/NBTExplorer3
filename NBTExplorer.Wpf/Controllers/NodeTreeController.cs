using System;
using System.Collections.Generic;
using System.Linq;
using NBTExplorer.Model;
using NBTExplorer.Wpf.ViewModels;
using Substrate.Nbt;

namespace NBTExplorer.Wpf.Controllers
{
    public class NodeTreeController
    {
        private MainViewModel _viewModel;

        public NodeTreeController(MainViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        #region Node Operations

        public void CreateNode(NodeViewModel parentViewModel, TagType type)
        {
            if (parentViewModel == null)
                return;

            var dataNode = parentViewModel.Node;
            if (!dataNode.CanCreateTag(type))
                return;

            if (dataNode.CreateNode(type))
            {
                parentViewModel.Refresh();
            }
        }

        public void DeleteNode(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            var dataNode = nodeViewModel.Node;
            if (!dataNode.CanDeleteNode)
                return;

            if (dataNode.DeleteNode())
            {
                // Remove from parent's children
                if (nodeViewModel.Parent != null)
                {
                    nodeViewModel.Parent.Children.Remove(nodeViewModel);
                }
                else
                {
                    // Remove from root nodes
                    _viewModel.RootNodes.Remove(nodeViewModel);
                }
            }
        }

        public void BatchDeleteNodes(IEnumerable<NodeViewModel> nodeViewModels)
        {
            if (nodeViewModels == null || !nodeViewModels.Any())
                return;

            foreach (var nodeViewModel in nodeViewModels.ToList())
            {
                DeleteNode(nodeViewModel);
            }
        }

        public void EditNode(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            var dataNode = nodeViewModel.Node;
            if (!dataNode.CanEditNode)
                return;

            if (dataNode.EditNode())
            {
                nodeViewModel.Refresh();
            }
        }

        public void RenameNode(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            var dataNode = nodeViewModel.Node;
            if (!dataNode.CanRenameNode)
                return;

            if (dataNode.RenameNode())
            {
                nodeViewModel.Refresh();
            }
        }

        public void RefreshNode(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            var dataNode = nodeViewModel.Node;
            if (!dataNode.CanRefreshNode)
                return;

            if (dataNode.RefreshNode())
            {
                nodeViewModel.Refresh();
            }
        }

        public void CopyNode(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            var dataNode = nodeViewModel.Node;
            if (!dataNode.CanCopyNode)
                return;

            dataNode.CopyNode();
        }

        public void BatchCopyNodes(IEnumerable<NodeViewModel> nodeViewModels)
        {
            if (nodeViewModels == null || !nodeViewModels.Any())
                return;

            // For now, we'll still only copy the first node as clipboard operations
            // are designed for single items in the current implementation
            var firstNode = nodeViewModels.First();
            CopyNode(firstNode);
        }

        public void CutNode(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            var dataNode = nodeViewModel.Node;
            if (!dataNode.CanCutNode)
                return;

            if (dataNode.CutNode())
            {
                // Remove from parent's children
                if (nodeViewModel.Parent != null)
                {
                    nodeViewModel.Parent.Children.Remove(nodeViewModel);
                }
                else
                {
                    // Remove from root nodes
                    _viewModel.RootNodes.Remove(nodeViewModel);
                }
            }
        }

        public void BatchCutNodes(IEnumerable<NodeViewModel> nodeViewModels)
        {
            if (nodeViewModels == null || !nodeViewModels.Any())
                return;

            // For now, we'll still only cut the first node as clipboard operations
            // are designed for single items in the current implementation
            var firstNode = nodeViewModels.First();
            CutNode(firstNode);
        }

        // Note: Full multi-item clipboard support would require:
        // 1. Modifying NbtClipboardController to support multiple items
        // 2. Updating DataNode.CopyNode() and CutNode() methods
        // 3. Implementing multi-item paste functionality
        // This is a more complex change that would require modifications to the NBTModel project

        public void PasteNode(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            var dataNode = nodeViewModel.Node;
            if (!dataNode.CanPasteIntoNode)
                return;

            if (dataNode.PasteNode())
            {
                nodeViewModel.Refresh();
            }
        }

        public void MoveNodeUp(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            var dataNode = nodeViewModel.Node;
            if (!dataNode.CanMoveNodeUp)
                return;

            if (dataNode.ChangeRelativePosition(-1))
            {
                nodeViewModel.Parent?.Refresh();
            }
        }

        public void MoveNodeDown(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            var dataNode = nodeViewModel.Node;
            if (!dataNode.CanMoveNodeDown)
                return;

            if (dataNode.ChangeRelativePosition(1))
            {
                nodeViewModel.Parent?.Refresh();
            }
        }

        #endregion

        #region Selection Operations

        public void SelectNode(DataNode dataNode)
        {
            _viewModel.SelectNode(dataNode);
        }

        #endregion

        #region Tree Operations

        public void ExpandNode(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            nodeViewModel.IsExpanded = true;
        }

        public void CollapseNode(NodeViewModel nodeViewModel)
        {
            if (nodeViewModel == null)
                return;

            nodeViewModel.IsExpanded = false;
        }

        #endregion

        #region Capability Checking

        public bool CanCreateTag(NodeViewModel nodeViewModel, TagType type)
        {
            return nodeViewModel?.Node.CanCreateTag(type) ?? false;
        }

        public bool CanDeleteNode(NodeViewModel nodeViewModel)
        {
            return nodeViewModel?.Node.CanDeleteNode ?? false;
        }

        public bool CanEditNode(NodeViewModel nodeViewModel)
        {
            return nodeViewModel?.Node.CanEditNode ?? false;
        }

        public bool CanRenameNode(NodeViewModel nodeViewModel)
        {
            return nodeViewModel?.Node.CanRenameNode ?? false;
        }

        public bool CanRefreshNode(NodeViewModel nodeViewModel)
        {
            return nodeViewModel?.Node.CanRefreshNode ?? false;
        }

        public bool CanCopyNode(NodeViewModel nodeViewModel)
        {
            return nodeViewModel?.Node.CanCopyNode ?? false;
        }

        public bool CanCutNode(NodeViewModel nodeViewModel)
        {
            return nodeViewModel?.Node.CanCutNode ?? false;
        }

        public bool CanPasteIntoNode(NodeViewModel nodeViewModel)
        {
            return nodeViewModel?.Node.CanPasteIntoNode ?? false;
        }

        public bool CanMoveNodeUp(NodeViewModel nodeViewModel)
        {
            return nodeViewModel?.Node.CanMoveNodeUp ?? false;
        }

        public bool CanMoveNodeDown(NodeViewModel nodeViewModel)
        {
            return nodeViewModel?.Node.CanMoveNodeDown ?? false;
        }

        #endregion
    }
}
