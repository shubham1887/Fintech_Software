using FluentValidation;

namespace FintechApp.Requests.ComplaintMaster
{
    public class UpdateComplaintMasterRequest
    {
        public int ComplaintMasterID { get; set; }
        public int UserID { get; set; }
        public int TransactionMasterID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CustomerRemark { get; set; } = string.Empty;
        public string AdminRemark { get; set; } = string.Empty;
        public string ComplaintStatus { get; set; } = string.Empty;
        public int TransactionMasterDOCInt { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? ComplaintType { get; set; }
        public string? NewConsumerNumber { get; set; }
        public int ProviderID { get; set; }
        public string? ProviderRequestID { get; set; }
        public string? ReferenceNo { get; set; }
    }

    public class UpdateComplaintMasterRequestValidator : AbstractValidator<UpdateComplaintMasterRequest>
    {
        public UpdateComplaintMasterRequestValidator()
        {
            RuleFor(x => x.ComplaintMasterID).GreaterThan(0);
            RuleFor(x => x.ComplaintStatus).NotEmpty();
        }
    }

    // Close out a complaint. Deliberately narrower than Update, which rewrites every column and
    // would blank the record's details if a caller only wanted to record an outcome.
    public class ResolveComplaintMasterRequest
    {
        public int ComplaintMasterID { get; set; }
        public string ComplaintStatus { get; set; } = string.Empty;
        public string AdminRemark { get; set; } = string.Empty;
    }

    public class ResolveComplaintMasterRequestValidator : AbstractValidator<ResolveComplaintMasterRequest>
    {
        public ResolveComplaintMasterRequestValidator()
        {
            RuleFor(x => x.ComplaintMasterID).GreaterThan(0);
            RuleFor(x => x.ComplaintStatus).NotEmpty();
        }
    }
}
