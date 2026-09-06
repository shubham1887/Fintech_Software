using FluentValidation;
namespace FintechApp.Requests.LocationLookUp
{
    public class UpdateLocationLookUpRequest { public int LocationLookUpID { get; set; } public int TableID { get; set; } public string SimCardDetails { get; set; } = string.Empty; public string Type { get; set; } = string.Empty; public string Location { get; set; } = string.Empty; public string IPAddress { get; set; } = string.Empty; public string Latitude { get; set; } = string.Empty; public string Longitude { get; set; } = string.Empty; }
    public class UpdateLocationLookUpRequestValidator : AbstractValidator<UpdateLocationLookUpRequest> { public UpdateLocationLookUpRequestValidator() { RuleFor(x => x.LocationLookUpID).GreaterThan(0); } }
}
