using FluentValidation;

namespace FintechApp.Requests.UserType
{
    public class GetUserTypeRequest
    {
        public int UserTypeID { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetUserTypeRequestValidator : AbstractValidator<GetUserTypeRequest>
    {
        public GetUserTypeRequestValidator() { }
    }
}
