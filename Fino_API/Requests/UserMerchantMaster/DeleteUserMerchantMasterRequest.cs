using FluentValidation;
namespace FintechApp.Requests.UserMerchantMaster { public class DeleteUserMerchantMasterRequest { public int UserMerchantMasterID { get; set; } } public class DeleteUserMerchantMasterRequestValidator : AbstractValidator<DeleteUserMerchantMasterRequest> { public DeleteUserMerchantMasterRequestValidator() { RuleFor(x => x.UserMerchantMasterID).GreaterThan(0); } } }
