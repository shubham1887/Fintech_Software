using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.Operator
{
    public class GetOperatorRequest
    {
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Opcode { get; set; } = string.Empty;
        // Null = no active/inactive filter (admin lists show OFF operators too, greyed out).
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetOperatorRequestValidator : AbstractValidator<GetOperatorRequest>
    {
        public GetOperatorRequestValidator()
        {
            // PageNumber
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue);

            // PageSize
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage(OperatorMessages.InvalidValue);

            // OperatorName
            RuleFor(x => x.OperatorName)
                .MaximumLength(100).WithMessage(OperatorMessages.InvalidLength);

            // Opcode
            RuleFor(x => x.Opcode)
                .MaximumLength(4).WithMessage(OperatorMessages.InvalidLength);
        }
    }
}