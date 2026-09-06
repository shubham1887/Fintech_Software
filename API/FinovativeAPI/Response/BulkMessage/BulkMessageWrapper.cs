using System.Collections.Generic;

namespace Project.Response.BulkMessage
{
    public class BulkMessageWrapper
    {
        private int _Count;

        public int Count { get => _Count; set => _Count = value; }

        public List<Project.Response.BulkMessage.ViewModel> list = new List<Project.Response.BulkMessage.ViewModel>();
    }
}