using System;

namespace Project.Processing
{
    public static class DecimalExtensions
    {
        public static decimal NDecimalPlaces(this decimal values, int places = 2)
        {
            return decimal.Round(values, places, MidpointRounding.AwayFromZero);
        }
    }
}