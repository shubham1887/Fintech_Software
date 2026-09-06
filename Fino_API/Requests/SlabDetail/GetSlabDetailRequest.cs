using FluentValidation;

namespace FintechApp.Requests.SlabDetail
{
    public class GetSlabDetailRequest
    {
        public int SlabDetailID { get; set; }
        public int SlabMasterID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
    }

    public class GetSlabDetailRequestValidator : AbstractValidator<GetSlabDetailRequest>
    {
        public GetSlabDetailRequestValidator() { }
    }
}
