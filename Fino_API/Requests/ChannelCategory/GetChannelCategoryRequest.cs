using FluentValidation;
namespace FintechApp.Requests.ChannelCategory { public class GetChannelCategoryRequest { public int ChannelCategoryID { get; set; } } public class GetChannelCategoryRequestValidator : AbstractValidator<GetChannelCategoryRequest> { public GetChannelCategoryRequestValidator() { RuleFor(x => x.ChannelCategoryID).GreaterThan(0); } } }
