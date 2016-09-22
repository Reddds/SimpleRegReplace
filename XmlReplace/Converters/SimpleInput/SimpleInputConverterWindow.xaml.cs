using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace XmlReplace.Converters.SimpleInput
{
    /// <summary>
    /// Логика взаимодействия для SimpleInputConverterWindow.xaml
    /// </summary>
    public partial class SimpleInputConverterWindow : Window
    {
        public SimpleInputConverterWindow()
        {
            InitializeComponent();

            if (Clipboard.ContainsText())
            {
                InputBox.Text = Clipboard.GetText();
            }
        }

        public string TextResult { get; set; }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbFilePath.Text))
            {
                TextResult = InputBox.Text;
                DialogResult = true;
            }

            else if (File.Exists(TbFilePath.Text))
            {
                using (var sr = new StreamReader(TbFilePath.Text))
                {
                    TextResult = sr.ReadToEnd();
                }
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Файл не найден!");
                return;
            }
            if (CbReplaceColonInStylesheetLink.IsChecked == true)
            {
                TextResult = TextResult.Replace("<?xml:stylesheet", "<?xml-stylesheet");
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != true)
                return;
            TbFilePath.Text = ofd.FileName;
        }
    }
}
