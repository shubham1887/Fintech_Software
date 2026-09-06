using System.Collections.Generic;

namespace Project.Response.IPWebSDK
{
    public partial class TransactionWebhookResponse
    {
        public string response_code { get; set; }

        public string response_msg { get; set; }

        public List<TransactionResponseParameters> transactions { get; set; }
    }

    public partial class TransactionResponseParameters
    {
        public string agent_id { get; set; }
    }
}