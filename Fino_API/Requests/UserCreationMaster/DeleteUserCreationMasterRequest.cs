using FluentValidation;

namespace FintechApp.Requests.UserCreationMaster
{
    public class DeleteUserCreationMasterRequest { public int UserCreationMasterID { get; set; } }
    public class DeleteUserCreationMasterRequestValidator : AbstractValidator<DeleteUserCreationMasterRequest>
    {
        public DeleteUserCreationMasterRequestValidator() { RuleFor(x => x.UserCreationMasterID).GreaterThan(0); }
    }
}
