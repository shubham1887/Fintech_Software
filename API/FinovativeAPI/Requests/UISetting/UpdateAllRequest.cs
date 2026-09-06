using Project.Processing;
using System.Collections.Generic;

namespace Project.Requests.UISetting
{
    public class UpdateAllRequest
    {
        public List<updateModel> list = new List<updateModel>();
    }

    public class updateModel
    {
        private int _UISettingMasterID;

        public int UISettingMasterID
        {
            get => _UISettingMasterID;
            set => _UISettingMasterID = value;
        }

        private string _Value;

        public string Value
        {
            get => _Value.TrimIfNotNull();
            set => _Value = value.TrimIfNotNull();
        }
    }
}