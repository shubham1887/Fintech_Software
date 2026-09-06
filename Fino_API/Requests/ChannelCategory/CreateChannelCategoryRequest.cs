using FluentValidation;
namespace FintechApp.Requests.ChannelCategory { public class CreateChannelCategoryRequest { public string Category { get; set; } = string.Empty; } public class CreateChannelCategoryRequestValidator : AbstractValidator<CreateChannelCategoryRequest> { public CreateChannelCategoryRequestValidator() {  } } }
