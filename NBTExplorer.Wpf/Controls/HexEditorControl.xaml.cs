using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NBTExplorer.Wpf.Controls
{
    public partial class HexEditorControl : UserControl, INotifyPropertyChanged
    {
        private byte[] _data;
        private int _bytesPerLine = 16;
        private int _currentPosition = 0;
        private bool _insertMode = false;
        private int _bytesPerElem = 1;

        public event EventHandler DataChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] Data
        {
            get { return _data; }
            set
            {
                _data = value;
                UpdateView();
                OnPropertyChanged("Data");
            }
        }

        public int BytesPerElem
        {
            get { return _bytesPerElem; }
            set
            {
                _bytesPerElem = value;
                UpdatePositionInfo();
                OnPropertyChanged("BytesPerElem");
            }
        }

        public bool InsertMode
        {
            get { return _insertMode; }
            set
            {
                _insertMode = value;
                ModeTextBlock.Text = "Mode: " + (_insertMode ? "Insert" : "Overwrite");
                OnPropertyChanged("InsertMode");
            }
        }

        public List<string> Addresses { get; private set; }
        public List<string> HexLines { get; private set; }
        public List<string> AsciiLines { get; private set; }

        public HexEditorControl()
        {
            InitializeComponent();
            Addresses = new List<string>();
            HexLines = new List<string>();
            AsciiLines = new List<string>();
            DataContext = this;
        }

        public void UpdateView()
        {
            Addresses.Clear();
            HexLines.Clear();
            AsciiLines.Clear();

            if (_data == null)
                return;

            int lines = (_data.Length + _bytesPerLine - 1) / _bytesPerLine;
            for (int i = 0; i < lines; i++)
            {
                int start = i * _bytesPerLine;
                int length = Math.Min(_bytesPerLine, _data.Length - start);

                // Address
                Addresses.Add("0x" + start.ToString("X4"));

                // Hex
                StringBuilder hexBuilder = new StringBuilder();
                for (int j = 0; j < _bytesPerLine; j++)
                {
                    if (j < length)
                        hexBuilder.Append(_data[start + j].ToString("X2") + " ");
                    else
                        hexBuilder.Append("   ");
                }
                HexLines.Add(hexBuilder.ToString().TrimEnd());

                // Ascii
                StringBuilder asciiBuilder = new StringBuilder();
                for (int j = 0; j < length; j++)
                {
                    byte b = _data[start + j];
                    asciiBuilder.Append((b >= 32 && b <= 126) ? (char)b : '.');
                }
                AsciiLines.Add(asciiBuilder.ToString());
            }

            OnPropertyChanged("Addresses");
            OnPropertyChanged("HexLines");
            OnPropertyChanged("AsciiLines");
            UpdatePositionInfo();
        }

        private void UpdatePositionInfo()
        {
            PositionTextBlock.Text = "Position: 0x" + _currentPosition.ToString("X4");
            ElementTextBlock.Text = "Element: " + (_currentPosition / _bytesPerElem);
        }

        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public byte[] GetRawData()
        {
            return _data;
        }

        public void SetRawData(byte[] data)
        {
            Data = data;
        }
    }
}