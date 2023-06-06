using System.Windows;

namespace ConfigManager
{
    /// <summary>
    /// Interaction logic for TextFileEditDialog.xaml
    /// </summary>
    public partial class TextFileEditDialog : Window
    {
        public string FileContent
        {
            get => FileContentTextBox.Text;
            set => FileContentTextBox.Text = value;
        }

        public TextFileEditDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
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
