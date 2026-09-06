using FluentValidation;
namespace FintechApp.Requests.ChannelCategory { public class DeleteChannelCategoryRequest { public int ChannelCategoryID { get; set; } } public class DeleteChannelCategoryRequestValidator : AbstractValidator<DeleteChannelCategoryRequest> { public DeleteChannelCategoryRequestValidator() { RuleFor(x => x.ChannelCategoryID).GreaterThan(0); } } }
