using FluentValidation;

namespace FintechApp.Requests.State
{
    public class GetStateRequest
    {
        public int StateID { get; set; }
        public string? StateName { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetStateRequestValidator : AbstractValidator<GetStateRequest>
    {
        public GetStateRequestValidator() { }
    }
}
