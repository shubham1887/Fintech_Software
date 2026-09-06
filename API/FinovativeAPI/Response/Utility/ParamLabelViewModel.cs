using Project.Processing;

namespace Project.Response.Utility
{
    public class ParamLabelViewModel
    {
        private string _ParamLabel1;

        public string ParamLabel1
        {
            get => _ParamLabel1.TrimIfNotNull();

            set => _ParamLabel1 = value.TrimIfNotNull();
        }

        private string _ParamLabel2;

        public string ParamLabel2
        {
            get => _ParamLabel2.TrimIfNotNull();

            set => _ParamLabel2 = value.TrimIfNotNull();
        }

        private string _ParamLabel3;

        public string ParamLabel3
        {
            get => _ParamLabel3.TrimIfNotNull();

            set => _ParamLabel3 = value.TrimIfNotNull();
        }

        private string _ParamLabel4;

        public string ParamLabel4
        {
            get => _ParamLabel4.TrimIfNotNull();

            set => _ParamLabel4 = value.TrimIfNotNull();
        }

        private string _ParamLabel5;

        public string ParamLabel5
        {
            get => _ParamLabel5.TrimIfNotNull();

            set => _ParamLabel5 = value.TrimIfNotNull();
        }
    }
}