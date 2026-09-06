using FluentValidation;

namespace FintechApp.Requests.ComplaintMaster
{
    public class DeleteComplaintMasterRequest
    {
        public int ComplaintMasterID { get; set; }
    }

    public class DeleteComplaintMasterRequestValidator : AbstractValidator<DeleteComplaintMasterRequest>
    {
        public DeleteComplaintMasterRequestValidator()
        {
            RuleFor(x => x.ComplaintMasterID).GreaterThan(0);
        }
    }
}
