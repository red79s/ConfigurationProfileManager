using System.Windows;

namespace ConfigManager
{
    /// <summary>
    /// Interaction logic for ProfileNameInputDialog.xaml
    /// </summary>
    public partial class ProfileNameInputDialog : Window
    {
        public string ProfileName
        {
            get { return ProfileNameTextBox.Text; }
            set { ProfileNameTextBox.Text = value; }
        }
        public ProfileNameInputDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
