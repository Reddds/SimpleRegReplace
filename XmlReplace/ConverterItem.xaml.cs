using System;
using System.Windows.Media.Imaging;

namespace XmlReplace
{
    /// <summary>
    /// Логика взаимодействия для ConverterItem.xaml
    /// </summary>
    public partial class ConverterItem
    {
//        private string _converterName;
        private string _converterFullName;
        private string _assemblyName;
        public ConverterItem()
        {
            InitializeComponent();
        }

        public string AssemblyName
        {
            get
            {
                return _assemblyName;
            }
        }

        public string ConverterFullName
        {
            get
            {
                return _converterFullName;
            }
        }

        public void Init(string assemblyName, string converterFullName, string className, string description)
        {
            _assemblyName = assemblyName;
            _converterFullName = converterFullName;
            TbDescription.Text = description;
//            var converterName = converter.GetType().Name;
            SetImage(className);
        }


        private void SetImage(string converterName)
        {
            var bi3 = new BitmapImage();
            bi3.BeginInit();//pack://application:,,,
            bi3.UriSource = new Uri("/XmlReplace;component/Resources/Images/" + converterName + ".png", UriKind.Relative);
            bi3.EndInit();

            MainIcon.Source = bi3;
        }
    }
}
