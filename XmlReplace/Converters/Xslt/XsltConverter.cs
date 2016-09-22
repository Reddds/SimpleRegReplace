using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using PropertyTools.DataAnnotations;

namespace XmlReplace
{
    public class XsltConverter : IsConverter
    {
        [XmlType("XsltConverterProperties")]
        public class XsltConverterProperties
        {
            [Category("Преобразование")]
            [DataType(DataType.MultilineText)]
            [Height(600, Double.NaN, 600)]
            [XmlElement("Xslt")]
            public string Xslt { get; set; }
        }

//        static XsltConverter()
//        {
//            ConverterDescription = "Xslt трансформатор";
//            ConverterIsOutput = false;
//            ConverterRequreParams = true;
//        }

        private XsltConverterProperties _properties = new XsltConverterProperties();

        private readonly XslCompiledTransform _xslt = new XslCompiledTransform();
        private readonly XsltArgumentList _argList = new XsltArgumentList();

        public event WriteMessageEventHandler WriteMessage;

        public XsltConverter()
        {
            ConverterType = ConverterTypes.Middle;
            ConverterRequreParams = true;

            _argList.XsltMessageEncountered += ArgListXsltMessageEncountered;            
        }

        public override void Init()
        {
            _xslt.Load(new XmlTextReader(new StringReader(_properties.Xslt)));
        }

        public override Object ParamsList
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = (XsltConverterProperties)value;
            }
        }

        void ArgListXsltMessageEncountered(object sender, XsltMessageEncounteredEventArgs e)
        {
            var evArgs = new WriteMessageEventArgs
            {
                MessageString = e.Message
            };
            OnShapeChanged(evArgs);
        }

        public override string Convert(string inpString)
        {
            using (var stringWriter = new StringWriter())
            {
                try
                {
                    _xslt.Transform(new XmlTextReader(new StringReader(inpString)), _argList, stringWriter);
                    return stringWriter.ToString();
                }
                catch (Exception e)
                {
                    throw new Exception("Ошибка трансформации:\n" + e);
                    return string.Empty;
                }
            }
        }

        protected virtual void OnShapeChanged(WriteMessageEventArgs e)
        {
            if (WriteMessage != null)
            {
                WriteMessage(this, e);
            }
        }

        public const string StaticDescription = "Xslt трансформатор";

        public override string Description
        {
            get
            {
                return StaticDescription;
            }
        }
    }
}
