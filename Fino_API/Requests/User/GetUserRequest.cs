using FluentValidation;
using FintechApp.Messages;

namespace FintechApp.Requests.User
{
    public class GetUserRequest
    {
        public int UserID { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public bool IsActive { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetUserRequestValidator : AbstractValidator<GetUserRequest>
    {
        public GetUserRequestValidator()
        {
            // PageNumber
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage(UserMessages.InvalidValue);

            // PageSize
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage(UserMessages.InvalidValue);

            // MobileNumber
            RuleFor(x => x.MobileNumber)
                .Matches(@"^\d{10}$").When(x => !string.IsNullOrEmpty(x.MobileNumber))
                .WithMessage(UserMessages.InvalidMobileNumber);
        }
    }
}