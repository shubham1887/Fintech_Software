using FluentValidation;
namespace FintechApp.Requests.LocationLookUp
{
    public class CreateLocationLookUpRequest { public int TableID { get; set; } public string SimCardDetails { get; set; } = string.Empty; public string Type { get; set; } = string.Empty; public string Location { get; set; } = string.Empty; public string IPAddress { get; set; } = string.Empty; public string Latitude { get; set; } = string.Empty; public string Longitude { get; set; } = string.Empty; }
    public class CreateLocationLookUpRequestValidator : AbstractValidator<CreateLocationLookUpRequest> { public CreateLocationLookUpRequestValidator() {  } }
}
