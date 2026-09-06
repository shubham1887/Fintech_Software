using FluentValidation;

namespace FintechApp.Requests.ComplaintMaster
{
    public class GetComplaintMasterRequest
    {
        public int ComplaintMasterID { get; set; }
        public int UserID { get; set; }
    }

    public class GetComplaintMasterRequestValidator : AbstractValidator<GetComplaintMasterRequest>
    {
        public GetComplaintMasterRequestValidator()
        {
            RuleFor(x => x.ComplaintMasterID).GreaterThan(0);
        }
    }
}
