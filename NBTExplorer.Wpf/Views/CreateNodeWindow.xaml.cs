using System;
using System.Windows;
using NBTModel.Interop;
using Substrate.Nbt;

namespace NBTExplorer.Wpf.Views
{
    public partial class CreateNodeWindow : Window
    {
        private readonly CreateTagFormData _data;
        private int _size;

        public CreateNodeWindow(CreateTagFormData data)
        {
            InitializeComponent();
            _data = data;
            
            Title = $"Create {_data.TagType}";

            if (!_data.HasName)
            {
                NameLabel.IsEnabled = false;
                NameTextBox.IsEnabled = false;
            }
            else
            {
                NameTextBox.Focus();
            }

            if (!IsTagSizedType(_data.TagType))
            {
                SizeLabel.IsEnabled = false;
                SizeTextBox.IsEnabled = false;
            }
        }

        private bool IsTagSizedType(TagType type)
        {
            switch (type)
            {
                case TagType.TAG_BYTE_ARRAY:
                case TagType.TAG_INT_ARRAY:
                case TagType.TAG_SHORT_ARRAY:
                case TagType.TAG_LONG_ARRAY:
                    return true;
                default:
                    return false;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                _data.TagNode = CreateTag();
                _data.TagName = _data.HasName ? NameTextBox.Text.Trim() : null;
                DialogResult = true;
                Close();
            }
        }

        private bool ValidateInput()
        {
            if (_data.HasName)
            {
                string name = NameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show(this, "You must provide a nonempty name.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (_data.RestrictedNames.Contains(name))
                {
                    MessageBox.Show(this, "Duplicate name provided.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (IsTagSizedType(_data.TagType))
            {
                if (!int.TryParse(SizeTextBox.Text.Trim(), out _size) || _size < 0)
                {
                    MessageBox.Show(this, "Size must be a valid non-negative integer.", "Invalid Size", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private TagNode CreateTag()
        {
            switch (_data.TagType)
            {
                case TagType.TAG_BYTE: return new TagNodeByte();
                case TagType.TAG_BYTE_ARRAY: return new TagNodeByteArray(new byte[_size]);
                case TagType.TAG_COMPOUND: return new TagNodeCompound();
                case TagType.TAG_DOUBLE: return new TagNodeDouble();
                case TagType.TAG_FLOAT: return new TagNodeFloat();
                case TagType.TAG_INT: return new TagNodeInt();
                case TagType.TAG_INT_ARRAY: return new TagNodeIntArray(new int[_size]);
                case TagType.TAG_LIST: return new TagNodeList(TagType.TAG_BYTE);
                case TagType.TAG_LONG: return new TagNodeLong();
                case TagType.TAG_LONG_ARRAY: return new TagNodeLongArray(new long[_size]);
                case TagType.TAG_SHORT: return new TagNodeShort();
                case TagType.TAG_SHORT_ARRAY: return new TagNodeShortArray(new short[_size]);
                case TagType.TAG_STRING: return new TagNodeString();
                default: return new TagNodeByte();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
