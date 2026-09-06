using Project.Model;
using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.Agent
{
    public class UserWrapper
    {
        public List<Location_PinCodeMaster> list = new List<Location_PinCodeMaster>();

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }
        
    }
}