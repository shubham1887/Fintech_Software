using FluentValidation;

namespace FintechApp.Requests.SlabDetail
{
    public class DeleteSlabDetailRequest
    {
        public int SlabDetailID { get; set; }
    }

    public class DeleteSlabDetailRequestValidator : AbstractValidator<DeleteSlabDetailRequest>
    {
        public DeleteSlabDetailRequestValidator()
        {
            RuleFor(x => x.SlabDetailID).GreaterThan(0);
        }
    }
}
