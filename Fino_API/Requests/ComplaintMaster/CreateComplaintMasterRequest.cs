using FluentValidation;

namespace FintechApp.Requests.ComplaintMaster
{
    public class CreateComplaintMasterRequest
    {
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

    public class CreateComplaintMasterRequestValidator : AbstractValidator<CreateComplaintMasterRequest>
    {
        public CreateComplaintMasterRequestValidator()
        {
            RuleFor(x => x.UserID).GreaterThan(0);
            RuleFor(x => x.ConsumerNumber).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.ComplaintStatus).NotEmpty();
        }
    }
}
