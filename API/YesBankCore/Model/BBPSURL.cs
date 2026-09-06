using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YesBankCore.Model
{

    internal class BBPSURL
    {
        public static readonly string BaseURL = "https://yesbbpsuat.transxt.in/";

        public static readonly string BillFetchURL = BaseURL + "portal/service/npciservice/fetch";
        public static readonly string BillPayURL = BaseURL + "portal/service/npciservice/pay";
        public static readonly string StatusCheckURL = BaseURL + "portal/service/npciservice/txnStatusCheck";
        public static readonly string ComplaintURL = BaseURL + "portal/service/npciservice/complain/register";
    }
}

