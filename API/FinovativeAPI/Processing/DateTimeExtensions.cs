using System;

namespace Project.Processing
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime StartOfMonth(this DateTime dt)
        {
            DateTime first = new DateTime(dt.Year, dt.Month, 1);

            return first;
        }

        public static DateTime LastOfMonth(this DateTime dt)
        {
            DateTime first = new DateTime(dt.Year, dt.Month, 1);
            DateTime last = first.AddMonths(1).AddDays(-1);

            return last;
        }

        public static double DifferenceInSeconds(DateTime DOC)
        {
            TimeSpan difference = DateTime.Now - DOC;

            double seconds = Math.Abs(difference.TotalSeconds);

            return seconds;
        }

        public static double DifferenceInMinutes(DateTime DOC)
        {
            TimeSpan difference = DateTime.Now - DOC;

            double seconds = Math.Abs(difference.TotalMinutes);

            return seconds;
        }

        public static int DateInIntegerFormat(DateTime DOC)
        {
            int toDaysDateInt = Convert.ToInt32(DOC.ToString(Project.StaticModel.Messages.DOCIntegerFomrat));

            return toDaysDateInt;
        }

        public static int DOCIntegerWithTwoDigitYear(DateTime DOC)
        {
            int toDaysDateInt = Convert.ToInt32(DOC.ToString(Project.StaticModel.Messages.DOCIntegerWithTwoDigitYear));

            return toDaysDateInt;
        }
    }
}