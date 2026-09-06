using FluentValidation;

namespace FintechApp.Requests.SlabMaster
{
    public class GetSlabMasterRequest
    {
        public int SlabMasterID { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetSlabMasterRequestValidator : AbstractValidator<GetSlabMasterRequest>
    {
        public GetSlabMasterRequestValidator() { }
    }
}
