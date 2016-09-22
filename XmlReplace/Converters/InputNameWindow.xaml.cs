using System.Windows;

namespace XmlReplace
{
    /// <summary>
    /// Логика взаимодействия для InputNameWindow.xaml
    /// </summary>
    public partial class InputNameWindow
    {
        public InputNameWindow()
        {
            InitializeComponent();
        }

        public string SetName
        {
            get
            {
                return TbSetName.Text;
            }
            set
            {
                TbSetName.Text = value;
            }
        }

        private void BOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
