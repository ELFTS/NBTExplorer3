using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NBTExplorer.Wpf.Views
{
    public partial class HexEditorWindow : Window
    {
        private int _bytesPerElem;
        private byte[] _data;
        private bool _modified;

        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public bool Modified
        {
            get { return _modified; }
        }

        public HexEditorWindow(string tagName, byte[] data, int bytesPerElem)
        {
            InitializeComponent();

            _bytesPerElem = bytesPerElem;
            _data = new byte[data.Length];
            Array.Copy(data, _data, data.Length);
            _modified = false;

            Title = "Editing: " + tagName;

            InitializeViews();
            SetupEventHandlers();
        }

        private void InitializeViews()
        {
            // Initialize text view
            textBox.Text = RawToText(_data, _bytesPerElem);

            // Initialize hex view
            hexEditor.Data = _data;
            hexEditor.BytesPerElem = _bytesPerElem;
        }

        private void SetupEventHandlers()
        {
            textBox.TextChanged += (s, e) => { _modified = true; };
            hexEditor.DataChanged += (s, e) => { _modified = true; };

            viewTabs.SelectionChanged += ViewTabs_SelectionChanged;

            importButton.Click += ImportButton_Click;
            exportButton.Click += ExportButton_Click;
            okButton.Click += OkButton_Click;
            cancelButton.Click += CancelButton_Click;
        }

        private void ViewTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewTabs.SelectedIndex == 0) // Text View
            {
                byte[] hexData = hexEditor.GetRawData();
                if (hexData != null)
                {
                    textBox.Text = RawToText(hexData, _bytesPerElem);
                }
            }
            else if (viewTabs.SelectedIndex == 1) // Hex View
            {
                byte[] textData = TextToRaw(textBox.Text, _bytesPerElem);
                if (textData != null)
                {
                    hexEditor.SetRawData(textData);
                }
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Binary Data|*|Text Data (*.txt)|*.txt",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        if (System.IO.Path.GetExtension(openFileDialog.FileName) == ".txt")
                        {
                            ImportText(openFileDialog.FileName);
                        }
                        else
                        {
                            ImportRaw(openFileDialog.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to import data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Binary Data|*|Text Data (*.txt)|*.txt",
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        if (System.IO.Path.GetExtension(saveFileDialog.FileName) == ".txt")
                        {
                            ExportText(saveFileDialog.FileName);
                        }
                        else
                        {
                            ExportRaw(saveFileDialog.FileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to export data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateData();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void UpdateData()
        {
            if (viewTabs.SelectedIndex == 0) // Text View
            {
                _data = TextToRaw(textBox.Text, _bytesPerElem);
            }
            else if (viewTabs.SelectedIndex == 1) // Hex View
            {
                _data = hexEditor.GetRawData();
            }
        }

        private void ImportRaw(string path)
        {
            using (FileStream fstr = File.OpenRead(path))
            {
                _data = new byte[fstr.Length];
                int bytesRead = fstr.Read(_data, 0, (int)fstr.Length);
                if (bytesRead < fstr.Length)
                {
                    Array.Resize(ref _data, bytesRead);
                }

                InitializeViews();
                _modified = true;
            }
        }

        private void ImportText(string path)
        {
            using (FileStream fstr = File.OpenRead(path))
            {
                byte[] raw = new byte[fstr.Length];
                int bytesRead = fstr.Read(raw, 0, (int)fstr.Length);
                if (bytesRead < fstr.Length)
                {
                    Array.Resize(ref raw, bytesRead);
                }

                string text = Encoding.UTF8.GetString(raw, 0, raw.Length);
                _data = TextToRaw(text, _bytesPerElem);

                InitializeViews();
                _modified = true;
            }
        }

        private void ExportRaw(string path)
        {
            UpdateData();

            using (FileStream fstr = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fstr.Write(_data, 0, _data.Length);
            }
        }

        private void ExportText(string path)
        {
            UpdateData();

            string text = RawToText(_data, _bytesPerElem);
            byte[] data = Encoding.UTF8.GetBytes(text);

            using (FileStream fstr = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fstr.Write(data, 0, data.Length);
            }
        }

        private static string RawToText(byte[] data, int bytesPerElem)
        {
            switch (bytesPerElem)
            {
                case 1: return RawToText(data, bytesPerElem, 16);
                case 2: return RawToText(data, bytesPerElem, 8);
                case 4: return RawToText(data, bytesPerElem, 4);
                case 8: return RawToText(data, bytesPerElem, 2);
                default: return RawToText(data, bytesPerElem, 1);
            }
        }

        private static string RawToText(byte[] data, int bytesPerElem, int elementsPerLine)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < data.Length; i += bytesPerElem)
            {
                if (data.Length - i < bytesPerElem)
                    break;

                switch (bytesPerElem)
                {
                    case 1:
                        builder.Append(((sbyte)data[i]).ToString());
                        break;

                    case 2:
                        builder.Append(BitConverter.ToInt16(data, i).ToString());
                        break;

                    case 4:
                        builder.Append(BitConverter.ToInt32(data, i).ToString());
                        break;

                    case 8:
                        builder.Append(BitConverter.ToInt64(data, i).ToString());
                        break;
                }

                if ((i / bytesPerElem) % elementsPerLine == elementsPerLine - 1)
                    builder.AppendLine();
                else
                    builder.Append("  ");
            }

            return builder.ToString();
        }

        private static byte[] TextToRaw(string text, int bytesPerElem)
        {
            string[] items = text.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);
            byte[] data = new byte[bytesPerElem * items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                int index = i * bytesPerElem;

                switch (bytesPerElem)
                {
                    case 1:
                        sbyte val1;
                        if (sbyte.TryParse(items[i], out val1))
                            data[index] = (byte)val1;
                        break;

                    case 2:
                        short val2;
                        if (short.TryParse(items[i], out val2))
                        {
                            byte[] buffer = BitConverter.GetBytes(val2);
                            Array.Copy(buffer, 0, data, index, 2);
                        }
                        break;

                    case 4:
                        int val4;
                        if (int.TryParse(items[i], out val4))
                        {
                            byte[] buffer = BitConverter.GetBytes(val4);
                            Array.Copy(buffer, 0, data, index, 4);
                        }
                        break;

                    case 8:
                        long val8;
                        if (long.TryParse(items[i], out val8))
                        {
                            byte[] buffer = BitConverter.GetBytes(val8);
                            Array.Copy(buffer, 0, data, index, 8);
                        }
                        break;
                }
            }

            return data;
        }
    }
}