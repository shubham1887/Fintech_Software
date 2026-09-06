using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.BBPSConfiguration
{
    public class BBPSWrapperResponse
    {
        private List<APIViewModel> _data;
        public List<APIViewModel> Data { get => _data; set => _data = value; }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
    }
}