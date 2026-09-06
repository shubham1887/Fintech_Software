using FluentValidation;
namespace FintechApp.Requests.MenuMaster
{
    public class CreateMenuMasterRequest { public decimal MenuSequence { get; set; } public string Name { get; set; } = string.Empty; public string Url { get; set; } = string.Empty; public string Icon { get; set; } = string.Empty; public string Description { get; set; } = string.Empty; public bool IsAdmin { get; set; } public bool IsSuperDistributer { get; set; } public bool IsDistributer { get; set; } public bool IsRetailer { get; set; } public bool IsAPIUser { get; set; } public bool IsSupport { get; set; } public int Level { get; set; } public string GUID { get; set; } = string.Empty; public string ParentMenuGUID { get; set; } = string.Empty; }
    public class CreateMenuMasterRequestValidator : AbstractValidator<CreateMenuMasterRequest> { public CreateMenuMasterRequestValidator() {  } }
}
