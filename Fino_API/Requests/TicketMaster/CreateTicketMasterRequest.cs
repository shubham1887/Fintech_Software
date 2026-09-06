using FintechApp.Messages;
using FluentValidation;

namespace FintechApp.Requests.TicketMaster
{
    public class CreateTicketMasterRequest
    {
        public int TicketCategoryID { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string UploadFilePath { get; set; } = string.Empty;
        public int UserID { get; set; }
        public int UserTypeID { get; set; }
    }

    public class CreateTicketMasterRequestValidator : AbstractValidator<CreateTicketMasterRequest>
    {
        public CreateTicketMasterRequestValidator()
        {
            RuleFor(x => x.TicketCategoryID)
                .GreaterThan(0).WithMessage(TicketMasterMessages.InvalidValue);

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage(TicketMasterMessages.RequiredField)
                .MaximumLength(500).WithMessage(TicketMasterMessages.InvalidLength);

            RuleFor(x => x.Body)
                .NotEmpty().WithMessage(TicketMasterMessages.RequiredField);

            RuleFor(x => x.UserID)
                .GreaterThan(0).WithMessage(TicketMasterMessages.InvalidValue);

            RuleFor(x => x.UserTypeID)
                .GreaterThan(0).WithMessage(TicketMasterMessages.InvalidValue);

            RuleFor(x => x.UploadFilePath)
                .MaximumLength(500).WithMessage(TicketMasterMessages.InvalidLength);
        }
    }
}
