using System;
using System.Windows;
using NBTModel.Interop;
using Substrate.Nbt;

namespace NBTExplorer.Wpf.Services
{
    public class NbtClipboardControllerWpf : INbtClipboardController
    {
        public bool ContainsData
        {
            get { return Clipboard.ContainsData(typeof(NbtClipboardDataWpf).FullName); }
        }

        public void CopyToClipboard(NbtClipboardData data)
        {
            NbtClipboardDataWpf dataWpf = new NbtClipboardDataWpf(data);
            Clipboard.SetData(typeof(NbtClipboardDataWpf).FullName, dataWpf);
        }

        public NbtClipboardData CopyFromClipboard()
        {
            NbtClipboardDataWpf clip = Clipboard.GetData(typeof(NbtClipboardDataWpf).FullName) as NbtClipboardDataWpf;
            if (clip == null)
                return null;

            TagNode node = clip.Node;
            if (node == null)
                return null;

            return new NbtClipboardData(clip.Name, node);
        }
    }

    [Serializable]
    public class NbtClipboardDataWpf
    {
        private string _name;
        private byte[] _data;

        public NbtClipboardDataWpf(NbtClipboardData data)
        {
            _name = data?.Name;
            if (data?.Node != null)
            {
                _data = NbtClipboardData.SerializeNode(data.Node);
            }
            else
            {
                _data = null;
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public TagNode Node
        {
            get { return _data != null ? NbtClipboardData.DeserializeNode(_data) : null; }
            set { _data = value != null ? NbtClipboardData.SerializeNode(value) : null; }
        }
    }
}