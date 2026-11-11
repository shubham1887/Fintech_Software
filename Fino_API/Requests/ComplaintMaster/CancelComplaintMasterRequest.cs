using FluentValidation;

namespace FintechApp.Requests.ComplaintMaster
{
    /// <summary>
    /// Withdraw a complaint you raised. This is the non-admin counterpart of Accept / Reject: it
    /// closes the complaint and moves no money, so a retailer who raised one by mistake (or whose
    /// customer has since been served) can take it off the queue without needing an admin.
    /// </summary>
    public class CancelComplaintMasterRequest
    {
        public int ComplaintMasterID { get; set; }

        /// <summary>Why it is being withdrawn — recorded against the complaint.</summary>
        public string Remark { get; set; } = string.Empty;
    }

    public class CancelComplaintMasterRequestValidator : AbstractValidator<CancelComplaintMasterRequest>
    {
        public CancelComplaintMasterRequestValidator()
        {
            RuleFor(x => x.ComplaintMasterID).GreaterThan(0);
        }
    }

    /// <summary>Look up the complaints raised against one transaction.</summary>
    public class GetComplaintByTransactionRequest
    {
        public int TransactionMasterID { get; set; }
    }

    public class GetComplaintByTransactionRequestValidator : AbstractValidator<GetComplaintByTransactionRequest>
    {
        public GetComplaintByTransactionRequestValidator()
        {
            RuleFor(x => x.TransactionMasterID).GreaterThan(0);
        }
    }
}
