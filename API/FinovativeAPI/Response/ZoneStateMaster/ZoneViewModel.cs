using Project.Processing;

namespace Project.Response.GSTState
{
    public class ZoneViewModel
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        public int ID { get; set; }
    }
}