using FluentValidation;
namespace FintechApp.Requests.UserServiceMapping
{
    public class CreateUserServiceMappingRequest { public int UserID { get; set; } public int ServiceID { get; set; } public bool IsActive { get; set; } }
    public class CreateUserServiceMappingRequestValidator : AbstractValidator<CreateUserServiceMappingRequest> { public CreateUserServiceMappingRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.ServiceID).GreaterThan(0); } }
}
