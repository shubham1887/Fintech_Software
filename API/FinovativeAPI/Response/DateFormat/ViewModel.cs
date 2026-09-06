using Project.Processing;

namespace Project.Response.DateFormat
{
    public class ViewModel
    {
        private string _DateFormat;

        public string DateFormat
        {
            get => _DateFormat.TrimIfNotNull();
            set => _DateFormat = value.TrimIfNotNull();
        }
    }
}