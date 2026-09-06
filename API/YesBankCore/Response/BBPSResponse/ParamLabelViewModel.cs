using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class ParamLabelViewModel
    {
        private string _Label1;

        public string Label1
        {
            get => _Label1.TrimIfNotNull();

            set => _Label1 = value.TrimIfNotNull();
        }

        private string _Label2;

        public string Label2
        {
            get => _Label2.TrimIfNotNull();

            set => _Label2 = value.TrimIfNotNull();
        }

        private string _Label3;

        public string Label3
        {
            get => _Label3.TrimIfNotNull();

            set => _Label3 = value.TrimIfNotNull();
        }

        private string _Label4;

        public string Label4
        {
            get => _Label4.TrimIfNotNull();

            set => _Label4 = value.TrimIfNotNull();
        }

        private string _Label5;

        public string Label5
        {
            get => _Label5.TrimIfNotNull();

            set => _Label5 = value.TrimIfNotNull();
        }
    }
}