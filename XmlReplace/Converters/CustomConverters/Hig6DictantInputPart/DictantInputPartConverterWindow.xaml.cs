using System;
using System.Windows;
using Microsoft.Win32;

namespace XmlReplace.Converters.CustomConverters.Hig6DictantInputPart
{
    /// <summary>
    /// Логика взаимодействия для InputPartConverterWindow.xaml
    /// </summary>
    public partial class DictantInputPartConverterWindow
    {
        public DictantInputPartConverterWindow()
        {
            InitializeComponent();
        }

//        public bool UseGrops
//        {
//            get
//            {
//                return cbUseGroups.IsChecked == true;
//            }
//        }

        public string ExerTitle
        {
            get { return TbTitle.Text; }
        }

        public long? SectionId
        {
            get
            {
                long secId;
                if(!long.TryParse(TbSectionId.Text, out secId))
                    throw new Exception("Не введён номер раздела! 0 - корневой раздел");
                if (secId == 0)
                    return null;
                return secId;
            }
        }

        public string XmlPath { get; set; }

        private void ChooseXmlOnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog {Filter = "*.xml|*.xml"};
            if (ofd.ShowDialog() != true)
            {
                DialogResult = false;
                return;
            }

            XmlPath = ofd.FileName;
            DialogResult = true;
        }
    }
}
