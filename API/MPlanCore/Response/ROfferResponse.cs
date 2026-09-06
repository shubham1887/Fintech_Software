using MplanCore.Controller;
using MplanCore.Model;
using Newtonsoft.Json;

namespace MplanCore.Response
{
    public class ROfferResponse
    {
        public class Record
        {
            private string _rs;

            public string rs
            {
                get => _rs.TrimIfNotNull();
                set => _rs = value.TrimIfNotNull();
            }

            private string _desc;

            public string desc
            {
                get => _desc.TrimIfNotNull();
                set => _desc = value.TrimIfNotNull();
            }

            private string _status;

            public string status
            {
                get => _status.TrimIfNotNull();
                set => _status = value.TrimIfNotNull();
            }

            private string _message;

            public string Message
            {
                get { return _message.TrimIfNotNull(); }
                set { _message = value.TrimIfNotNull(); }
            }
        }

        public class Response
        {
            //private string tel;
            //private string @operator;

            [JsonConverter(typeof(SingleOrArrayConverter<Record>))]
            private List<Record> _records;

            public List<Record> records
            {
                get => _records;
                set => _records = value;
            }

            public Response()
            {
                _records = new List<Record>();
            }

            private int _status;

            public int status
            {
                get => _status;
                set => _status = value;
            }

            private double _time;

            public double time
            {
                get => _time;
                set => _time = value;
            }


            
        }
    }
}