namespace FintechApp.Messages
{
    public static class UserMessages
    {
        // User CRUD Messages
        public const string UserNotFound = "User not found.";
        public const string UserCreated = "User created successfully.";
        public const string UserUpdated = "User updated successfully.";
        public const string UserDeleted = "User deleted successfully.";
        public const string UserAlreadyExists = "User already exists.";
        public const string MobileNumberExists = "Mobile number already registered.";
        public const string EmailExists = "Email already registered.";
        public const string InvalidMobileNumber = "Invalid mobile number.";
        public const string InvalidEmail = "Invalid email address.";
        public const string InvalidUserType = "This action is only available for API users.";

        // Validation Messages
        public const string RequiredField = "This field is required.";
        public const string InvalidFormat = "Invalid format.";
        public const string InvalidLength = "Invalid length.";
        public const string InvalidValue = "Invalid value.";
    }
}