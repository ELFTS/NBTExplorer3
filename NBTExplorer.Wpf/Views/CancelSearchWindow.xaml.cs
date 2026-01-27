using System.Windows;

namespace NBTExplorer.Wpf.Views
{
    public partial class CancelSearchWindow : Window
    {
        public CancelSearchWindow()
        {
            InitializeComponent();
        }

        public string SearchPath
        {
            get { return SearchPathText.Text; }
            set { SearchPathText.Text = value; }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
