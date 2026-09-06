using FluentValidation;

namespace FintechApp.Requests.UserSlabMapping
{
    public class UpdateUserSlabMappingRequest
    {
        public int UserSlabMappingID { get; set; }
        public int UserID { get; set; }
        public int SlabMasterID { get; set; }
    }

    public class UpdateUserSlabMappingRequestValidator : AbstractValidator<UpdateUserSlabMappingRequest>
    {
        public UpdateUserSlabMappingRequestValidator()
        {
            RuleFor(x => x.UserSlabMappingID).GreaterThan(0);
            RuleFor(x => x.UserID).GreaterThan(0);
            RuleFor(x => x.SlabMasterID).GreaterThan(0);
        }
    }
}
