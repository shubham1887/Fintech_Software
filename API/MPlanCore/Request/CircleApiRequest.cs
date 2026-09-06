using MplanCore.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MplanCore.Request
{
    public class CircleApiRequest
    {
        private string _apimember_id;

        public string apimember_id
        {
            get => _apimember_id.TrimIfNotNull();
            set => _apimember_id = value.TrimIfNotNull();
        }

        private string _api_password;

        public string api_password
        {
            get => _api_password.TrimIfNotNull();
            set => _api_password = value.TrimIfNotNull();
        }

        private string _Mobileno;

        public string Mobileno
        {
            get => _Mobileno.TrimIfNotNull();
            set => _Mobileno = value.TrimIfNotNull();
        }
    }
}
