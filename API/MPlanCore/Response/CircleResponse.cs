using MplanCore.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MplanCore.Response
{
    public class CircleResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
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
       
    }
}
