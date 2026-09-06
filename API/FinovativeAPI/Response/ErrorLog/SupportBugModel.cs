using Project.Processing;

namespace Project.Response.ErrorLog
{
    public class SupportBugModel
    {
        private string _Exception;

        public string Exception
        {
            get => _Exception.TrimIfNotNull();
            set => _Exception = value.TrimIfNotNull();
        }

        private string _Company;

        public string Company
        {
            get => _Company.TrimIfNotNull();
            set => _Company = value.TrimIfNotNull();
        }
    }
}