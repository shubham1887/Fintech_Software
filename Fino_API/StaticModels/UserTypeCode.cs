namespace FintechApp.StaticModels
{
    // Numeric user-type codes used by the role-based sidebar menu (as supplied by the frontend).
    public static class UserTypeCode
    {
        public const int AllUsers = 0;
        public const int Admin = 1;
        public const int SuperDistributor = 2;
        public const int Distributor = 3;
        public const int Retailer = 4;
        public const int ApiUser = 5;
        public const int SupportUser = 6;
        public const int Client = 7;
        public const int Accountant = 9;
        public const int SDFOS = 10;
        public const int DFOS = 11;
        public const int DeactiveUsers = 999;
    }
}
