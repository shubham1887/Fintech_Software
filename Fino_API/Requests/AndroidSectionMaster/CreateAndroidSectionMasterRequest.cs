using FluentValidation;
namespace FintechApp.Requests.AndroidSectionMaster { public class CreateAndroidSectionMasterRequest { public string SectionName { get; set; } = string.Empty; } public class CreateAndroidSectionMasterRequestValidator : AbstractValidator<CreateAndroidSectionMasterRequest> { public CreateAndroidSectionMasterRequestValidator() {  } } }
