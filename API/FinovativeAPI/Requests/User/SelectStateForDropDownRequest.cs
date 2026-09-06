using Project.Processing;

namespace Project.Requests.User
{
    public class SelectStateForDropDownRequest
    {
        private string _Name;

        public string Name
        {
            get => _Name.ToTitleCase();
            set => _Name = value.ToTitleCase();
        }
    }
}