using System;
using System.ComponentModel;
using System.Windows;
using NBTExplorer.Model;
using NBTExplorer.Wpf.ViewModels;

namespace NBTExplorer.Wpf.Views
{
    public partial class FindBlockWindow : Window, INotifyPropertyChanged
    {
        private class CoordinateGroup
        {
            public int? Region;
            public int? Chunk;
            public int? Block;
            public int? LocalChunk;
            public int? LocalBlock;
        }

        private bool _inUpdate;
        private CoordinateGroup _groupX;
        private CoordinateGroup _groupZ;
        private DataNode _searchRoot;
        private DataNode _searchResult;

        public event PropertyChangedEventHandler PropertyChanged;

        public FindBlockWindow(DataNode searchRoot)
        {
            InitializeComponent();
            DataContext = this;

            _searchRoot = searchRoot;
            _groupX = new CoordinateGroup();
            _groupZ = new CoordinateGroup();

            // Initialize with default values
            RegionX = "0";
            RegionZ = "0";

            FindCommand = new RelayCommand(p => Find());
            CancelCommand = new RelayCommand(p => Cancel());
        }

        public DataNode Result => _searchResult;

        #region Properties

        private string _regionX;
        public string RegionX
        {
            get => _regionX;
            set
            {
                if (_regionX != value)
                {
                    _regionX = value;
                    OnPropertyChanged(nameof(RegionX));
                    if (!_inUpdate)
                    {
                        int region = 0;
                        if (int.TryParse(value, out region))
                        {
                            _inUpdate = true;
                            ApplyRegion(_groupX, value, true);
                            _inUpdate = false;
                            OnPropertyChanged(nameof(CanFind));
                        }
                    }
                }
            }
        }

        private string _regionZ;
        public string RegionZ
        {
            get => _regionZ;
            set
            {
                if (_regionZ != value)
                {
                    _regionZ = value;
                    OnPropertyChanged(nameof(RegionZ));
                    if (!_inUpdate)
                    {
                        int region = 0;
                        if (int.TryParse(value, out region))
                        {
                            _inUpdate = true;
                            ApplyRegion(_groupZ, value, true);
                            _inUpdate = false;
                            OnPropertyChanged(nameof(CanFind));
                        }
                    }
                }
            }
        }

        private string _chunkX;
        public string ChunkX
        {
            get => _chunkX;
            set
            {
                if (_chunkX != value)
                {
                    _chunkX = value;
                    OnPropertyChanged(nameof(ChunkX));
                    if (!_inUpdate)
                    {
                        int chunk = 0;
                        if (int.TryParse(value, out chunk))
                        {
                            _inUpdate = true;
                            ApplyChunk(_groupX, value, true);
                            _inUpdate = false;
                            OnPropertyChanged(nameof(CanFind));
                        }
                    }
                }
            }
        }

        private string _chunkZ;
        public string ChunkZ
        {
            get => _chunkZ;
            set
            {
                if (_chunkZ != value)
                {
                    _chunkZ = value;
                    OnPropertyChanged(nameof(ChunkZ));
                    if (!_inUpdate)
                    {
                        int chunk = 0;
                        if (int.TryParse(value, out chunk))
                        {
                            _inUpdate = true;
                            ApplyChunk(_groupZ, value, true);
                            _inUpdate = false;
                            OnPropertyChanged(nameof(CanFind));
                        }
                    }
                }
            }
        }

        private string _blockX;
        public string BlockX
        {
            get => _blockX;
            set
            {
                if (_blockX != value)
                {
                    _blockX = value;
                    OnPropertyChanged(nameof(BlockX));
                    if (!_inUpdate)
                    {
                        int block = 0;
                        if (int.TryParse(value, out block))
                        {
                            _inUpdate = true;
                            ApplyBlock(_groupX, value, true);
                            _inUpdate = false;
                            OnPropertyChanged(nameof(CanFind));
                        }
                    }
                }
            }
        }

        private string _blockZ;
        public string BlockZ
        {
            get => _blockZ;
            set
            {
                if (_blockZ != value)
                {
                    _blockZ = value;
                    OnPropertyChanged(nameof(BlockZ));
                    if (!_inUpdate)
                    {
                        int block = 0;
                        if (int.TryParse(value, out block))
                        {
                            _inUpdate = true;
                            ApplyBlock(_groupZ, value, true);
                            _inUpdate = false;
                            OnPropertyChanged(nameof(CanFind));
                        }
                    }
                }
            }
        }

        private string _localChunkX;
        public string LocalChunkX
        {
            get => _localChunkX;
            set
            {
                if (_localChunkX != value)
                {
                    _localChunkX = value;
                    OnPropertyChanged(nameof(LocalChunkX));
                    if (!_inUpdate)
                    {
                        int localChunk = 0;
                        if (int.TryParse(value, out localChunk))
                        {
                            _inUpdate = true;
                            ApplyLocalChunk(_groupX, value, true);
                            _inUpdate = false;
                            OnPropertyChanged(nameof(CanFind));
                        }
                    }
                }
            }
        }

        private string _localChunkZ;
        public string LocalChunkZ
        {
            get => _localChunkZ;
            set
            {
                if (_localChunkZ != value)
                {
                    _localChunkZ = value;
                    OnPropertyChanged(nameof(LocalChunkZ));
                    if (!_inUpdate)
                    {
                        int localChunk = 0;
                        if (int.TryParse(value, out localChunk))
                        {
                            _inUpdate = true;
                            ApplyLocalChunk(_groupZ, value, true);
                            _inUpdate = false;
                            OnPropertyChanged(nameof(CanFind));
                        }
                    }
                }
            }
        }

        private string _localBlockX;
        public string LocalBlockX
        {
            get => _localBlockX;
            set
            {
                if (_localBlockX != value)
                {
                    _localBlockX = value;
                    OnPropertyChanged(nameof(LocalBlockX));
                    if (!_inUpdate)
                    {
                        int localBlock = 0;
                        if (int.TryParse(value, out localBlock))
                        {
                            _inUpdate = true;
                            ApplyLocalBlock(_groupX, value, true);
                            _inUpdate = false;
                            OnPropertyChanged(nameof(CanFind));
                        }
                    }
                }
            }
        }

        private string _localBlockZ;
        public string LocalBlockZ
        {
            get => _localBlockZ;
            set
            {
                if (_localBlockZ != value)
                {
                    _localBlockZ = value;
                    OnPropertyChanged(nameof(LocalBlockZ));
                    if (!_inUpdate)
                    {
                        int localBlock = 0;
                        if (int.TryParse(value, out localBlock))
                        {
                            _inUpdate = true;
                            ApplyLocalBlock(_groupZ, value, true);
                            _inUpdate = false;
                            OnPropertyChanged(nameof(CanFind));
                        }
                    }
                }
            }
        }

        public bool CanFind => _groupX.LocalChunk.HasValue && _groupZ.LocalChunk.HasValue;
        public RelayCommand FindCommand { get; }
        public RelayCommand CancelCommand { get; }

        #endregion

        #region Methods

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DataNode Search(DataNode node)
        {
            if (node is DirectoryDataNode dirNode)
            {
                if (!dirNode.IsExpanded)
                    dirNode.Expand();

                foreach (var subNode in dirNode.Nodes)
                {
                    var result = Search(subNode);
                    if (result != null)
                        return result;
                }
                return null;
            }
            else if (node is RegionFileDataNode regionNode)
            {
                int rx, rz;
                if (!RegionFileDataNode.RegionCoordinates(regionNode.NodePathName, out rx, out rz))
                    return null;
                if (rx != _groupX.Region.Value || rz != _groupZ.Region.Value)
                    return null;

                if (!regionNode.IsExpanded)
                    regionNode.Expand();

                foreach (var subNode in regionNode.Nodes)
                {
                    var result = Search(subNode);
                    if (result != null)
                        return result;
                }
                return null;
            }
            else if (node is RegionChunkDataNode chunkNode)
            {
                if (chunkNode.X != _groupX.LocalChunk.Value || chunkNode.Z != _groupZ.LocalChunk.Value)
                    return null;
                return chunkNode;
            }
            return null;
        }

        private int Mod(int a, int b)
        {
            return ((a % b) + b) % b;
        }

        #region Coordinate Conversion Methods

        private string RegionFromBlock(int block) => ((block >> 4) >> 5).ToString();
        private string ChunkFromBlock(int block) => (block >> 4).ToString();
        private string LocalChunkFromBlock(int block) => Mod(block >> 4, 32).ToString();
        private string LocalBlockFromBlock(int block) => Mod(block, 16).ToString();

        private string RegionFromChunk(int chunk) => (chunk >> 5).ToString();
        private string BlockFromChunk(int chunk) => $"({chunk * 16} to {(chunk + 1) * 16 - 1})";
        private string BlockFromChunk(int chunk, int localBlock) => (chunk * 16 + localBlock).ToString();
        private string LocalChunkFromChunk(int chunk) => Mod(chunk, 32).ToString();
        private string LocalBlockFromChunk(int chunk) => "(0 to 15)";

        private string ChunkFromRegion(int region) => $"({region * 32} to {(region + 1) * 32 - 1})";
        private string BlockFromRegion(int region) => $"({region * 32 * 16} to {(region + 1) * 32 * 16 - 1})";
        private string LocalChunkFromRegion(int region) => "(0 to 31)";
        private string LocalBlockFromRegion(int region) => "(0 to 15)";

        private string ChunkFromLocalChunk(int region, int localChunk) => (region * 32 + localChunk).ToString();
        private string BlockFromLocalChunk(int region, int localChunk) => BlockFromChunk(region * 32 + localChunk);

        private string BlockFromLocalBlock(int region, int localChunk, int localBlock) => (region * 32 * 16 + localChunk * 16 + localBlock).ToString();

        #endregion

        #region Apply Methods

        private void ApplyRegion(CoordinateGroup group, string value, bool primary)
        {
            group.Region = ParseInt(value);

            if (primary && group.Region.HasValue)
            {
                if (group.LocalChunk.HasValue)
                {
                    ChunkX = ChunkFromLocalChunk(group.Region.Value, group.LocalChunk.Value);
                    if (group.LocalBlock.HasValue)
                        BlockX = BlockFromLocalBlock(group.Region.Value, group.LocalChunk.Value, group.LocalBlock.Value);
                    else
                    {
                        BlockX = BlockFromLocalChunk(group.Region.Value, group.LocalChunk.Value);
                        LocalBlockX = LocalBlockFromChunk(group.LocalChunk.Value);
                    }
                }
                else
                {
                    ChunkX = ChunkFromRegion(group.Region.Value);
                    BlockX = BlockFromRegion(group.Region.Value);
                    LocalChunkX = LocalChunkFromRegion(group.Region.Value);
                    LocalBlockX = LocalBlockFromRegion(group.Region.Value);
                }
            }
        }

        private void ApplyChunk(CoordinateGroup group, string value, bool primary)
        {
            group.Chunk = ParseInt(value);

            if (primary && group.Chunk.HasValue)
            {
                RegionX = RegionFromChunk(group.Chunk.Value);
                LocalChunkX = LocalChunkFromChunk(group.Chunk.Value);
                if (group.LocalBlock.HasValue)
                    BlockX = BlockFromChunk(group.Chunk.Value, group.LocalBlock.Value);
                else
                {
                    BlockX = BlockFromChunk(group.Chunk.Value);
                    LocalBlockX = LocalBlockFromChunk(group.Chunk.Value);
                }
            }
        }

        private void ApplyBlock(CoordinateGroup group, string value, bool primary)
        {
            group.Block = ParseInt(value);

            if (primary && group.Block.HasValue)
            {
                RegionX = RegionFromBlock(group.Block.Value);
                ChunkX = ChunkFromBlock(group.Block.Value);
                LocalChunkX = LocalChunkFromBlock(group.Block.Value);
                LocalBlockX = LocalBlockFromBlock(group.Block.Value);
            }
        }

        private void ApplyLocalChunk(CoordinateGroup group, string value, bool primary)
        {
            group.LocalChunk = ParseInt(value);

            if (primary && group.LocalChunk.HasValue)
            {
                if (group.Region.HasValue)
                {
                    ChunkX = ChunkFromLocalChunk(group.Region.Value, group.LocalChunk.Value);
                    if (group.LocalBlock.HasValue)
                        BlockX = BlockFromLocalBlock(group.Region.Value, group.LocalChunk.Value, group.LocalBlock.Value);
                    else
                    {
                        BlockX = BlockFromLocalChunk(group.Region.Value, group.LocalChunk.Value);
                        LocalBlockX = LocalBlockFromChunk(group.LocalChunk.Value);
                    }
                }
                else
                {
                    RegionX = "(ANY)";
                    ChunkX = "(ANY)";
                    BlockX = "(ANY)";
                    LocalBlockX = "(0 to 15)";
                }
            }
        }

        private void ApplyLocalBlock(CoordinateGroup group, string value, bool primary)
        {
            group.LocalBlock = ParseInt(value);

            if (primary && group.LocalBlock.HasValue)
            {
                if (group.Region.HasValue && group.LocalChunk.HasValue)
                {
                    ChunkX = ChunkFromLocalChunk(group.Region.Value, group.LocalChunk.Value);
                    BlockX = BlockFromLocalBlock(group.Region.Value, group.LocalChunk.Value, group.LocalBlock.Value);
                }
                else
                {
                    RegionX = "(ANY)";
                    ChunkX = "(ANY)";
                    BlockX = "(ANY)";
                }
            }
        }

        private int? ParseInt(string value)
        {
            int parsedValue;
            if (int.TryParse(value, out parsedValue))
                return parsedValue;
            return null;
        }

        #endregion

        #endregion

        #region Commands

        private void Find()
        {
            _searchResult = Search(_searchRoot);
            DialogResult = true;
            Close();
        }

        private void Cancel()
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}
