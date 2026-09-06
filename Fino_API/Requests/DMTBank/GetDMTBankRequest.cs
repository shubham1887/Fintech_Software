using FluentValidation;

namespace FintechApp.Requests.DMTBank
{
    public class GetDMTBankRequest
    {
        public int DMTBankID { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetDMTBankRequestValidator : AbstractValidator<GetDMTBankRequest>
    {
        public GetDMTBankRequestValidator() { }
    }
}
