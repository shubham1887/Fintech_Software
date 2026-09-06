using FluentValidation;
namespace FintechApp.Requests.UserServiceMapping
{
    public class GetUserServiceMappingRequest { public int UserServiceMappingID { get; set; } public int UserID { get; set; } }
    public class GetUserServiceMappingRequestValidator : AbstractValidator<GetUserServiceMappingRequest> { public GetUserServiceMappingRequestValidator() { RuleFor(x => x.UserServiceMappingID).GreaterThan(0); } }
}
