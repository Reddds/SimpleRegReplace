using System;

namespace XmlReplace.Converters.SimpleOutput
{
    class SimpleOutputConverter : IsConverter
    {
        public event WriteMessageEventHandler WriteMessage;

//        static SimpleOutputConverter()
//        {
//            ConverterDescription = "Простой вывод текста";
//            ConverterIsOutput = true;
//            ConverterRequreParams = false;
//        }

        public SimpleOutputConverter()
        {
            ConverterType = ConverterTypes.Output;
            ConverterRequreParams = false;
        }

        public override void ConvertToOutput(string inpString)
        {
            //SimpleOutputRemoveXmlDeclaration
            var resXml = IsUtils.XmlUtils.GetIndentedXml(inpString, true);
            if (Properties.Settings.Default.SimpleOutputRemoveXmlDeclaration)
            {
                var firstNewLine = resXml.IndexOf(Environment.NewLine);
                if (firstNewLine > 0)
                    resXml = resXml.Remove(0, firstNewLine);
            }
            var win = new SimpleOutputConverterWindow
            {
                DataContext = resXml
            };
            win.ShowDialog();
        }

        public const string StaticDescription = "Простой вывод текста";

        public override string Description
        {
            get
            {
                return StaticDescription;
            }
        }

    }
}
