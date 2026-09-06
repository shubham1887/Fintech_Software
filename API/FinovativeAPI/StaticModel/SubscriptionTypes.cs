namespace Project.StaticModel
{
    public class SubscriptionTypes
    {
        public static readonly string Monthly = "Monthly";
        public static readonly string Yearly = "Yearly";
        public static readonly string Quarterly = "Quarterly";
    }

    public enum SubscriptionDays
    {
        Monthly = 30,
        Yearly = 365,
        Quarterly = 90,

        MonthlyDue = 30-3,
        YearlyDue = 365-3,
        QuarterlyDue = 90-3,

    }
}
