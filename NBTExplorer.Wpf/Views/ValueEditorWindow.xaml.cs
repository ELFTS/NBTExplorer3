using System;
using System.Windows;
using NBTModel.Interop;
using Substrate.Nbt;

namespace NBTExplorer.Wpf.Views
{
    public partial class ValueEditorWindow : Window
    {
        private readonly TagScalarFormData _data;

        public ValueEditorWindow(TagScalarFormData data)
        {
            InitializeComponent();
            _data = data;
            
            TagType type = _data.Tag.GetTagType();
            Title = $"Edit {type}";
            PromptLabel.Text = $"Enter {type} value:";
            ValueTextBox.Text = _data.Tag.ToString();
            ValueTextBox.SelectAll();
            ValueTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyValue();
                DialogResult = true;
                Close();
            }
            catch (FormatException)
            {
                MessageBox.Show(this, "The value format is invalid.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (OverflowException)
            {
                MessageBox.Show(this, "The value is out of range.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyValue()
        {
            string text = ValueTextBox.Text;
            switch (_data.Tag.GetTagType())
            {
                case TagType.TAG_BYTE:
                    _data.Tag.ToTagByte().Data = (byte)sbyte.Parse(text);
                    break;
                case TagType.TAG_SHORT:
                    _data.Tag.ToTagShort().Data = short.Parse(text);
                    break;
                case TagType.TAG_INT:
                    _data.Tag.ToTagInt().Data = int.Parse(text);
                    break;
                case TagType.TAG_LONG:
                    _data.Tag.ToTagLong().Data = long.Parse(text);
                    break;
                case TagType.TAG_FLOAT:
                    _data.Tag.ToTagFloat().Data = float.Parse(text);
                    break;
                case TagType.TAG_DOUBLE:
                    _data.Tag.ToTagDouble().Data = double.Parse(text);
                    break;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
