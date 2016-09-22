using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace XmlReplace
{
    public class SplitOutputConverter : IsConverter
    {
        private SplitOutputConverterProperties _properties;

        [XmlType("SplitOutputConverterProperties")]
        public class SplitOutputConverterProperties
        {
            [Category("Х Путь для резки")]
            [XmlElement("XPathSplit")]
            public string XPathSplit { get; set; }
        }

        public event WriteMessageEventHandler WriteMessage;

        public SplitOutputConverter()
        {
            ConverterType = ConverterTypes.Output;
            ConverterRequreParams = true;

            _properties = new SplitOutputConverterProperties();
        }

        public override void ConvertToOutput(string inpString)
        {
            var xDoc = new XmlDocument();
            
            xDoc.LoadXml(inpString);
            var splitNodes = xDoc.SelectNodes(_properties.XPathSplit);
            if (splitNodes == null)
            {
                MessageBox.Show("Не делится");
                return;
            }

            var outDic = new Dictionary<string, string>();

            // Костылями попахивает
            // Клонируем документы и удаляем из них разделяемые части
            var splitNodesCount = splitNodes.Count;
            for (var i = 0; i < splitNodesCount; i++)
            {
//                var node = splitNodes[i];
                var newDoc = xDoc.Clone();
                var newDocSplitNodes = newDoc.SelectNodes(_properties.XPathSplit);
                Debug.Assert(newDocSplitNodes != null);
                var parentNode = newDocSplitNodes[0].ParentNode;
                Debug.Assert(parentNode != null);
                for (var j = splitNodesCount - 1; j >= 0; j--)
                {
                    if (i == j)
                        continue;
                    parentNode.RemoveChild(newDocSplitNodes[j]);
                }

                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                XmlTextWriter xtw = null;
                try
                {
                    xtw = new XmlTextWriter(sw)
                    {
                        Formatting = Formatting.Indented
                    };
                    newDoc.WriteTo(xtw);
                }
                finally
                {
                    if (xtw != null)
                        xtw.Close();
                }
                //return sb.ToString();
                outDic.Add(i.ToString(), sb.ToString());
            }

//            var counter = 0;
//            foreach (XmlNode node in splitNodes)
//            {
//                outDic.Add((counter++).ToString(), node.InnerXml);
//            }

            var win = new SplitOutputConverterPropertiesWindow()
            {
                DataContext = outDic
            };
            win.ShowDialog();
        }

        public const string StaticDescription = "Разделение на несколько частей";

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
                _properties = (SplitOutputConverterProperties)value;
            }
        }

    }
}
