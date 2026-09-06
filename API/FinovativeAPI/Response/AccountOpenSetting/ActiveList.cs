using Project.Processing;

namespace Project.Response.AccountOpenSetting
{
    public class ActiveList
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }
    }
}