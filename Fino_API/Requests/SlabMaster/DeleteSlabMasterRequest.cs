using FluentValidation;

namespace FintechApp.Requests.SlabMaster
{
    public class DeleteSlabMasterRequest
    {
        public int SlabMasterID { get; set; }
    }

    public class DeleteSlabMasterRequestValidator : AbstractValidator<DeleteSlabMasterRequest>
    {
        public DeleteSlabMasterRequestValidator()
        {
            RuleFor(x => x.SlabMasterID).GreaterThan(0);
        }
    }
}
