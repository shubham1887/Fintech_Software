using Project.Processing;

namespace Project.Response.UISetting
{
    public class ViewModel
    {
        private int _UISettingMasterID;

        public int UISettingMasterID
        {
            get => _UISettingMasterID;
            set => _UISettingMasterID = value;
        }

        private string _Key;

        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }

        private string _Value;

        public string Value
        {
            get => _Value.TrimIfNotNull();
            set => _Value = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }
    }
}