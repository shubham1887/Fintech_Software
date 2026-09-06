namespace Project.Response.BBPSConfiguration
{
    public class APIViewModel
    {
        private string _ParameterName;

        public string ParameterName
        {
            get => _ParameterName;
            set => _ParameterName = value;
        }

        private string _Type;

        public string Type
        {
            get => _Type;
            set => _Type = value;
        }

        private bool _IsRequired;

        public bool IsRequired
        {
            get => _IsRequired;
            set => _IsRequired = value;
        }

        private bool _IsDisplay;

        public bool IsDisplay
        {
            get => _IsDisplay;
            set => _IsDisplay = value;
        }
    }
}