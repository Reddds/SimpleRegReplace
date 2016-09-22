using System;

namespace XmlReplace.Converters.SimpleInput
{
    class SimpleInputConverter : IsConverter
    {
        public SimpleInputConverter()
        {
            ConverterType = ConverterTypes.Input;
            ConverterRequreParams = false;
        }

        public override string InputData()
        {
            var win = new SimpleInputConverterWindow();
            if (win.ShowDialog() != true)
                throw new Exception("Ошибка ввода");
            return win.TextResult;
        }

        public const string StaticDescription = "Простой ввод текста или файла";

        public override string Description
        {
            get
            {
                return StaticDescription;
            }
        }

    }
}
