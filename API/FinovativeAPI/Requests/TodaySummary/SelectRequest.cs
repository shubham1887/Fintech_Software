using Project.Processing;

namespace Project.Requests.TodaySummary
{
    public class SelectRequest
    {
        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
        }
    }
}