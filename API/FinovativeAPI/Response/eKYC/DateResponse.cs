using MplanCore.Controller;

namespace Projet.Response.eKYC
{
    public class DateResponse
    {
        private string _DateIntstring;

        public string DateIntstring
        {
            get => _DateIntstring.TrimIfNotNull();
            set => _DateIntstring = value.TrimIfNotNull();
        }
        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
        }
    }
}
