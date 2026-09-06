using Project.Processing;
using Project.Response.UserOperatorLImit;
using System.Collections.Generic;

namespace Project.Response.UserOperatorLimit
{
    public class UserOperatorLimitResponse
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        public string ServiceName
        {
            get;
            internal set;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private List<UserList> _UserList;
        public List<UserList> UserList { get => _UserList; set => _UserList = value; }
    }
}