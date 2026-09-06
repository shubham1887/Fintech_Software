using FluentValidation;
namespace FintechApp.Requests.UserServiceMapping
{
    public class DeleteUserServiceMappingRequest { public int UserServiceMappingID { get; set; } }
    public class DeleteUserServiceMappingRequestValidator : AbstractValidator<DeleteUserServiceMappingRequest> { public DeleteUserServiceMappingRequestValidator() { RuleFor(x => x.UserServiceMappingID).GreaterThan(0); } }
}
