using System.Windows;

namespace XmlReplace.Converters.SimpleOutput
{
    /// <summary>
    /// Логика взаимодействия для SimpleOutputConverterWindow.xaml
    /// </summary>
    public partial class SimpleOutputConverterWindow : Window
    {
        public SimpleOutputConverterWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(XmlOut.Text);
        }
    }
}
