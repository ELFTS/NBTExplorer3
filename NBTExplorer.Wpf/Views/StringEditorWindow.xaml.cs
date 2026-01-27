using System.Windows;
using NBTModel.Interop;

namespace NBTExplorer.Wpf.Views
{
    public partial class StringEditorWindow : Window
    {
        private readonly StringFormData _data;

        public StringEditorWindow(StringFormData data)
        {
            InitializeComponent();
            _data = data;
            ValueTextBox.Text = data.Value ?? "";
            ValueTextBox.SelectAll();
            ValueTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_data.AllowEmpty && string.IsNullOrEmpty(ValueTextBox.Text))
            {
                MessageBox.Show(this, "The value cannot be empty.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_data is RestrictedStringFormData restrictedData)
            {
                if (restrictedData.RestrictedValues.Contains(ValueTextBox.Text) && ValueTextBox.Text != restrictedData.Value)
                {
                    MessageBox.Show(this, "The name is already in use.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            _data.Value = ValueTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
