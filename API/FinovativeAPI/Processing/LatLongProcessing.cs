using System;

namespace Project.Processing
{
    public class LatLongProcessing
    {
        string lat = "18.5204";
        string lonng = "73.8567";
        public double getRandomLatitude(string Value)
        {
            return getLat(Value, "13");
        }

        public double getRandomLongitude(string Value)
        {
            return getLong(Value, "80");
        }

        private string getRandom(string startValue)
        {
            Random random = new Random();
            return $"{startValue}.{random.Next(0, 999990).ToString().PadLeft(6, '0')}";
        }

        private double getLat(string Value, string startValue)
        {
           
            if (Value == null)
            {
                Value = lat;// getRandom(startValue);
            }

            if (Value.TrimIfNotNull().Length <= 0)
            {
                Value = lat;//getRandom(startValue);
            }

            double OutVal;
            double.TryParse(Value, out OutVal);

            if (double.IsNaN(OutVal) || double.IsInfinity(OutVal))
            {
                return Convert.ToDouble(lat);
                //return Convert.ToDouble(getRandom(startValue));
            }
            return OutVal;
        }

        private double getLong(string Value, string startValue)
        {
           
            if (Value == null)
            {
                Value = lonng;// getRandom(startValue);
            }

            if (Value.TrimIfNotNull().Length <= 0)
            {
                Value = lonng;// getRandom(startValue);
            }

            double OutVal;
            double.TryParse(Value, out OutVal);

            if (double.IsNaN(OutVal) || double.IsInfinity(OutVal))
            {
                return Convert.ToDouble(lonng);
                //return Convert.ToDouble(getRandom(startValue));
            }
            return OutVal;
        }
       
    }
}
