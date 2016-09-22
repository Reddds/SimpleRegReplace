using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace XmlReplace.Converters.ReplaceValue
{
    public class ReplaceValueConverter : IsConverter
    {
        private ReplaceValueConverterProperties _properties;

        [XmlType("ReplaceValueConverterProperties")]
        public class ReplaceValueConverterProperties
        {
            [Category("Х Путь для замены")]
            [XmlElement("XPath")]
            public string XPath { get; set; }

            [Category("Формат замены, {0} - текущее значение")]
            [XmlElement("ReplaceFormat")]
            public string ReplaceFormat { get; set; }
        }

        public ReplaceValueConverter()
        {
            ConverterType = ConverterTypes.Middle;
            ConverterRequreParams = true;

            _properties = new ReplaceValueConverterProperties();
        }

        public override string Convert(string inpString)
        {
            if(string.IsNullOrEmpty(inpString))
                return string.Empty;

            var xDoc = new XmlDocument();

            xDoc.LoadXml(inpString);
            var foundNodes = xDoc.SelectNodes(_properties.XPath);
            if (foundNodes == null || foundNodes.Count == 0)
                return inpString;
            foreach (XmlNode foundNode in foundNodes)
            {
                foundNode.InnerText = string.Format(_properties.ReplaceFormat, foundNode.InnerText);
            }
            return xDoc.OuterXml;
        }

        public const string StaticDescription = "Замена или преобразование значения тега или атрибута";

        public override string Description
        {
            get
            {
                return StaticDescription;
            }
        }

        public override Object ParamsList
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = (ReplaceValueConverterProperties)value;
            }
        }

    }
}
