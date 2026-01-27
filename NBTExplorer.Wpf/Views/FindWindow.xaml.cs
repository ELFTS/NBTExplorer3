using System.Windows;
using System.Windows.Controls;

namespace NBTExplorer.Wpf.Views
{
    public partial class FindWindow : Window
    {
        public FindWindow()
        {
            InitializeComponent();
        }

        public bool MatchName => NameCheckBox.IsChecked == true;
        public bool MatchValue => ValueCheckBox.IsChecked == true;
        public string NameToken => NameTextBox.Text;
        public string ValueToken => ValueTextBox.Text;

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void NameCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Optional: Logic when checked
        }

        private void ValueCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Optional: Logic when checked
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NameTextBox.Text))
            {
                NameCheckBox.IsChecked = true;
            }
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ValueTextBox.Text))
            {
                ValueCheckBox.IsChecked = true;
            }
        }
    }
}
