using FluentValidation;

namespace FintechApp.Requests.UserCreationMaster
{
    public class GetUserCreationMasterRequest { public int UserCreationMasterID { get; set; } public int UserID { get; set; } }
    public class GetUserCreationMasterRequestValidator : AbstractValidator<GetUserCreationMasterRequest>
    {
        public GetUserCreationMasterRequestValidator() { RuleFor(x => x.UserCreationMasterID).GreaterThan(0); }
    }
}
