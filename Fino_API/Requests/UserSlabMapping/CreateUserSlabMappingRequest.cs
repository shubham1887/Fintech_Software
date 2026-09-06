using FluentValidation;

namespace FintechApp.Requests.UserSlabMapping
{
    public class CreateUserSlabMappingRequest
    {
        public int UserID { get; set; }
        public int SlabMasterID { get; set; }
    }

    public class CreateUserSlabMappingRequestValidator : AbstractValidator<CreateUserSlabMappingRequest>
    {
        public CreateUserSlabMappingRequestValidator()
        {
            RuleFor(x => x.UserID).GreaterThan(0);
            RuleFor(x => x.SlabMasterID).GreaterThan(0);
        }
    }
}
