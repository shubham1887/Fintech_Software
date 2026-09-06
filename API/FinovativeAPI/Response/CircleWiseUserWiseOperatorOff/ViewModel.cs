using Project.Processing;
using Project.Requests.CircleWiseUserWiseOperatorOff;
using System.Collections.Generic;

namespace Project.Response.CircleWiseUserWiseOperatorOff
{
    public class ViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
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

        private List<StateMasterList> _List;

        public List<StateMasterList> StateList
        {
            get => _List;
            set => _List = value;
        }

        private int _CircleWiseUserWiseOperatorOffID;

        public int CircleWiseUserWiseOperatorOffID
        {
            get => _CircleWiseUserWiseOperatorOffID;
            set => _CircleWiseUserWiseOperatorOffID = value;
        }
    }
}