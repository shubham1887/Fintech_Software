using MplanCore.Controller;
using MplanCore.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Project.Response.PlanAPI
{
    public class ROfferResponse
    {
      //public List<Record> records = new List<Record>();

        private List<Record> _records;

        public List<Record> records
        {
            get => _records;
            set => _records = value;
        }


        private string _Message;
        public string Message
        {
            get { return _Message.TrimIfNotNull(); }
            set { _Message = value.TrimIfNotNull(); }
        }
        public ROfferResponse()
        {
            records = new List<Record>();
        }

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

          
        }
      
    }
}
