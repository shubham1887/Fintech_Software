using MplanCore.Controller;

namespace MplanCore.Response
{
    public class CircleApiResponse
    {

        private string _STATUS;

        public string STATUS
        {
            get => _STATUS.TrimIfNotNull();
            set => _STATUS = value.TrimIfNotNull();
        }
        
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Mobile;

        public string Mobile
        {
            get => _Mobile.TrimIfNotNull();
            set => _Mobile = value.TrimIfNotNull();
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }
        
        private string _Circle;

        public string Circle
        {
            get => _Circle.TrimIfNotNull();
            set => _Circle = value.TrimIfNotNull();
        }
        
        private string _CircleCode;

        public string CircleCode
        {
            get => _CircleCode.TrimIfNotNull();
            set => _CircleCode = value.TrimIfNotNull();
        }

    }
}
