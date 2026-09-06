using FluentValidation;
namespace FintechApp.Requests.UserServiceMapping
{
    public class UpdateUserServiceMappingRequest { public int UserServiceMappingID { get; set; } public int UserID { get; set; } public int ServiceID { get; set; } public bool IsActive { get; set; } }
    public class UpdateUserServiceMappingRequestValidator : AbstractValidator<UpdateUserServiceMappingRequest> { public UpdateUserServiceMappingRequestValidator() { RuleFor(x => x.UserServiceMappingID).GreaterThan(0); } }
}
