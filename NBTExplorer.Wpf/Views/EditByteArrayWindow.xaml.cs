using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using NBTModel.Interop;

namespace NBTExplorer.Wpf.Views
{
    public partial class EditByteArrayWindow : Window
    {
        private readonly ByteArrayFormData _data;

        public EditByteArrayWindow(ByteArrayFormData data)
        {
            InitializeComponent();
            _data = data;
            
            Title = $"Edit Data - {_data.NodeName}";
            
            // Convert byte[] to Hex String
            HexTextBox.Text = BitConverter.ToString(_data.Data).Replace("-", " ");
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Binary Data|*.*|Text Files|*.txt",
                Title = "Import Data"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(dialog.FileName);
                    HexTextBox.Text = BitConverter.ToString(bytes).Replace("-", " ");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Error importing data: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] bytes = GetBytesFromHex();
                var dialog = new SaveFileDialog
                {
                    Filter = "Binary Data|*.*",
                    Title = "Export Data",
                    FileName = _data.NodeName
                };

                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(dialog.FileName, bytes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error exporting data: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private byte[] GetBytesFromHex()
        {
            string hex = HexTextBox.Text.Replace(" ", "").Replace("\n", "").Replace("\r", "");
            if (hex.Length % 2 != 0)
            {
                throw new Exception("Hex string must have an even number of characters.");
            }

            int byteCount = hex.Length / 2;
            byte[] bytes = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] bytes = GetBytesFromHex();

                // Validation for element alignment
                if (bytes.Length % _data.BytesPerElement != 0)
                {
                     MessageBox.Show(this, $"Data length must be a multiple of {_data.BytesPerElement} bytes.", "Invalid Data Size", MessageBoxButton.OK, MessageBoxImage.Warning);
                     return;
                }

                _data.Data = bytes;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error parsing hex data: {ex.Message}", "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
