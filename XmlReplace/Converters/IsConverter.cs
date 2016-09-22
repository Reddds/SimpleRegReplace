using System;

namespace XmlReplace
{
    public class IsConverter
    {
        public class LogEventArgs : EventArgs
        {
            public enum MsgTypes {Text, Info, Warning, Error}
            public string Msg { get; private set; }
            public MsgTypes MsgType { get; private set; }

            public LogEventArgs(string msg, MsgTypes msgType)
            {
                Msg = msg;
                MsgType = msgType;
            }
        }
        public delegate void LogHandler(object sender, LogEventArgs e);
        public event LogHandler OnLog;

        protected void Log(string msg, LogEventArgs.MsgTypes msgType = LogEventArgs.MsgTypes.Text)
        {
            // Make sure someone is listening to event
            if (OnLog == null) return;

            OnLog(this, new LogEventArgs(msg, msgType));
        }
        public enum ConverterTypes {Input, Output, Middle}
        event WriteMessageEventHandler WriteMessage;
//        protected static string ConverterDescription;
        //protected ConverterTypes ConverterType;
        protected bool ConverterRequreParams;
        public virtual void Init(){}
        /// <summary>
        /// Получить объект настроек
        /// Если уже проинициализировано, то с данными
        /// </summary>
        public virtual Object ParamsList { get; set; }

        /// <summary>
        /// Имя 
        /// </summary>
        public string Name { get; set; }

        public virtual string Convert(string inpString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Поледнее преобразование для выводных конвертеров
        /// </summary>
        /// <param name="inpString"></param>
        public virtual void ConvertToOutput(string inpString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ввод данных
        /// </summary>
        public virtual string InputData()
        {
            throw new NotImplementedException();
        }

        public bool RequreParams
        {
            get
            {
                return ConverterRequreParams;
            }
        }

        public ConverterTypes ConverterType { get; protected set; }

//        /// <summary>
//        /// Данный конвертер ставится в конце для вывода полученных результатов
//        /// </summary>
//        public bool IsOutput
//        {
//            get
//            {
//                return ConverterIsOutput;
//            }
//        }
//
//        /// <summary>
//        /// Данный конвертер ставится в начале для ввода данных
//        /// </summary>
//        public bool IsInput
//        {
//            get
//            {
//                return ConverterIsInput;
//            }
//        }

        /// <summary>
        /// Краткое описание конвертера
        /// </summary>
        public virtual string Description 
        {
            get
            {
                return "Описание";
            } 
        }
    }

    public class ConverterParam
    {
        public string Name;
        public Type Type;
        public Object Value;
    }

    public class WriteMessageEventArgs : EventArgs
    {
        public string MessageString;
    }

    public delegate void WriteMessageEventHandler(object sender, WriteMessageEventArgs e);
}
