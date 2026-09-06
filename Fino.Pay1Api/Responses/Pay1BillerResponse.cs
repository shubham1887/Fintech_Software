namespace Fino.Pay1Api.Responses
{
    // Pay1's biller catalogue (operation_type 7): every biller Pay1 serves, with the inputs each
    // one asks the customer for. Used to populate the operator + BBPS field masters instead of
    // typing hundreds of billers by hand.
    public class Pay1BillerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public List<Pay1Biller> Billers { get; set; } = new();

        public static Pay1BillerResponse Fail(string message) => new() { Success = false, Message = message };
    }

    public class Pay1Biller
    {
        /// <summary>Pay1 product id — the value sent as "operator" on fetch and payment.</summary>
        public string ProductID { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>Biller supports a live bill enquiry before payment.</summary>
        public bool BillFetch { get; set; }

        /// <summary>Customer may pay an amount other than the fetched bill.</summary>
        public bool AmountChange { get; set; }

        /// <summary>0 = exact, 1 = exact and above, 2 = exact and below, 3 = any amount.</summary>
        public string AmountExactness { get; set; } = string.Empty;

        public bool AfterDueDate { get; set; }

        /// <summary>Customer inputs in the order Pay1 lists them: the first is the consumer number.</summary>
        public List<Pay1BillerField> Fields { get; set; } = new();
    }

    public class Pay1BillerField
    {
        public string Label { get; set; } = string.Empty;

        /// <summary>Pay1 query parameter this field feeds: accountNumber / param / param1 / amount.</summary>
        public string Param { get; set; } = string.Empty;

        public string Sample { get; set; } = string.Empty;
        public string Regex { get; set; } = string.Empty;

        /// <summary>Input kind as Pay1 describes it (text / number / date / select).</summary>
        public string Input { get; set; } = string.Empty;
    }
}
