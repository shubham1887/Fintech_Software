using Irony.Parsing;
using Project.BAL;
using Project.DAL;
using Project.Response.TransactionReport;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Project.Processing
{
    public class Util
    {
        public bool isDevelopment()
        {
            bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").EqualsIgnoreCase("development");

            return isDevelopment;
        }

        internal static bool isRoleAdminOrSDORDist(int UserTypeID)
        {
            if (UserTypeID == (int)UserType.Admin)
            {
                return true;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                return true;
            }

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                return true;
            }
            return false;
        }

        internal static bool isRoleRetailerOrClientOrAPIUser(int UserTypeID)
        {
            if (UserTypeID == (int)UserType.Retailer)
            {
                return true;
            }
            if (UserTypeID == (int)UserType.APIUser)
            {
                return true;
            }

            if (UserTypeID == (int)UserType.Client)
            {
                return true;
            }
            return false;
        }

        internal static bool isRoleAdminOrSupport(int UserTypeID)
        {
            if (UserTypeID == (int)UserType.Admin)
            {
                return true;
            }

            if (UserTypeID == (int)UserType.Support)
            {
                return true;
            }
            return false;
        }

      
        internal static bool isRoleSuperDistOrDist(int UserTypeID)
        {
            if (UserTypeID == (int)UserType.Distributer)
            {
                return true;
            }

            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                return true;
            }
            return false;
        }

        internal static bool isDFOSOrSDFOS(int UserTypeID)
        {
            if (UserTypeID == (int)UserType.DFOS)
            {
                return true;
            }

            if (UserTypeID == (int)UserType.SDFOS)
            {
                return true;
            }
            return false;
        }

        internal string GetUniqueRandomNumber(int length = 15)
        {
            var getNumbers = (from t in Guid.NewGuid().ToString()
                              where char.IsDigit(t)
                              select t).ToArray();

            string sttNumber = string.Join("", getNumbers);

            sttNumber = sttNumber.TrimStart(new Char[] { '0' });// remove starting zero, some api not considering starting zero

            return (sttNumber.Length >= 16) ? sttNumber.Substring(0, length) : sttNumber;
        }

        internal string GetUniqueNumber(int length = 14)
        {
            // Ensure length is at least 4 to accommodate the alphabetic characters
            if (length < 4)
            {
                throw new ArgumentException("Length must be at least 4 to accommodate alphabetic characters.");
            }
            // Generate 4 random alphabetic characters
            var random = new Random();
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var alphaPart = new char[4];
            for (int i = 0; i < 4; i++)
            {
                alphaPart[i] = alphabet[random.Next(alphabet.Length)];
            }

            // Generate numeric part from GUID and trim leading zeros
            var guid = Guid.NewGuid().ToString();
            var numPart = new StringBuilder();
            foreach (var ch in guid)
            {
                if (char.IsDigit(ch))
                {
                    numPart.Append(ch);
                }
            }
            string numericString = numPart.ToString().TrimStart('0');

            string result = new string(alphaPart) + numericString;

            return result.Length >= length ? result.Substring(0, length) : result;
        }

        public string GetRandomNumber()
        {
            string randomNumber = GetUniqueRandomNumber();

            Project.Model.TransactionMaster master = new Project.DAL.TransactionDAL().SelectByProviderRequestIDTodayOnly(randomNumber);

            if (master != null)
            {
                randomNumber = GetUniqueRandomNumber();

                master = new Project.DAL.TransactionDAL().SelectByProviderRequestIDTodayOnly(randomNumber);

                if (master != null)
                {
                    return randomNumber = "";
                }
            }

            return randomNumber;
        }

        private string GetRequestID(string ranNumber)
        {
            int RequestIDLength = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.RequestIDLength), 15);

            if (RequestIDLength <= 8 || RequestIDLength >= 15)
                RequestIDLength = 15;

            if (ranNumber.Length >= RequestIDLength)
            {
                ranNumber = ranNumber.Substring(0, RequestIDLength);
                return ranNumber;
            }

            return ranNumber;
        }

        private string GetUniqueRandomNumberWithDatePrefix()
        {
            int docint = DateTimeExtensions.DOCIntegerWithTwoDigitYear(DateTime.Now);

            return $"{docint}{GetUniqueRandomNumber(8)}";
        }

        public string GetRandomNumberWithDatePrefix()
        {
            string randomNumber = GetUniqueRandomNumberWithDatePrefix();

            randomNumber = GetRequestID(randomNumber);

            Project.Model.TransactionMaster master = new Project.DAL.TransactionDAL().SelectByProviderRequestIDTodayOnly(randomNumber);

            if (master != null)
            {
                randomNumber = GetUniqueRandomNumberWithDatePrefix();
                randomNumber = GetRequestID(randomNumber);

                master = new Project.DAL.TransactionDAL().SelectByProviderRequestIDTodayOnly(randomNumber);

                if (master != null)
                {
                    return randomNumber = "";
                }
            }

            return randomNumber;
        }

        public string GetRandomPassword()
        {
            int lengthOfPassword = 8;
            string valid = ("1234567890");
            StringBuilder strB = new StringBuilder(100);
            Random random = new Random();
            while (lengthOfPassword-- > 0)
            {
                strB.Append(valid[random.Next(valid.Length)]);
            }
            return strB.ToString();
        }

        public string GetRandomTransactionID()
        {
            int lengthOfPassword = 12;
            string valid = ("1234567890ABCDEFGHI");
            StringBuilder strB = new StringBuilder(100);
            Random random = new Random();
            while (lengthOfPassword-- > 0)
            {
                strB.Append(valid[random.Next(valid.Length)]);
            }
            return strB.ToString() + "M";
        }

        public string GetRandomOTP()
        {
            int lengthOfPassword = 6;
            string valid = ("1234567890");
            StringBuilder strB = new StringBuilder(100);
            Random random = new Random();
            while (lengthOfPassword-- > 0)
            {
                strB.Append(valid[random.Next(valid.Length)]);
            }
            return strB.ToString();
        }

        public DateTime ParseRequestDate(string StrDate)
        {
            try
            {
                CultureInfo enUS = new CultureInfo("en-US");
                var dt = StrDate;
                //30/03/2019
                //15-04-2019
                //23/04/2019 12:38:35 PM

                DateTime dateValue;

                // Scenario #1
                if (DateTime.TryParseExact(dt, "dd/MM/yyyy", enUS, DateTimeStyles.None, out dateValue))
                    return dateValue;

                // Scenario #2
                if (DateTime.TryParseExact(dt, "dd/MM/yyyy hh:mm:ss tt", enUS, DateTimeStyles.None, out dateValue))
                    return dateValue;

                // Scenario #3
                if (DateTime.TryParseExact(dt, "dd-MM-yyyy", enUS, DateTimeStyles.None, out dateValue))
                    return dateValue;

                // Scenario #4   
                if (DateTime.TryParseExact(dt, "dd-MM-yyyy hh:mm:ss tt", enUS, DateTimeStyles.None, out dateValue))
                    return dateValue;

                // Scenario #5
                if (DateTime.TryParseExact(dt, "dd-MM-yyyy hh.mm.ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                    return dateValue;


                // Scenario #6
                if (DateTime.TryParseExact(dt, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                    return dateValue;

                // Scenario #7
                if (DateTime.TryParseExact(dt, "dd-MM-yyyy hh:mm:ss tt", enUS, DateTimeStyles.AdjustToUniversal, out dateValue))
                    return dateValue;

                // Scenario #8
                if (DateTime.TryParseExact(dt, "MM/dd/yyyy hh:mm:ss tt", enUS, DateTimeStyles.AdjustToUniversal, out dateValue))
                    return dateValue;

                // Scenario #9
                if (DateTime.TryParseExact(dt, "MM/dd/yyyy HH:mm:ss", enUS, DateTimeStyles.None, out dateValue))
                    return dateValue;

                // Scenario #10
                if (DateTime.TryParseExact(dt, "M/dd/yyyy hh:mm", enUS, DateTimeStyles.None, out dateValue))
                    return dateValue;

                // Scenario #11
                if (DateTime.TryParseExact(dt, "MM/dd/yyyy hh:mm", enUS, DateTimeStyles.None, out dateValue))
                    return dateValue;

                // Scenario #12
                if (DateTime.TryParseExact(dt, "MM/dd/yyyy hh:mm:ss tt zzz", enUS, DateTimeStyles.None, out dateValue))
                    return dateValue;

                // Scenario #13
                if (DateTime.TryParseExact(dt, "MM/dd/yyyy hh:mm:ss tt zzz", enUS, DateTimeStyles.AdjustToUniversal, out dateValue))
                    return dateValue;

                // Scenario #14
                if (DateTime.TryParseExact(dt, "MM/dd/yyyy hh:mm", enUS, DateTimeStyles.None, out dateValue))
                    return dateValue;

                // Scenario #15
                if (DateTime.TryParseExact(dt, "MM/dd/yyyy hh:mm", enUS, DateTimeStyles.None, out dateValue))
                    return dateValue;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return Convert.ToDateTime(StrDate);
        }

        public string GetLeadingZeroNumber(int No, int length)
        {
            int Len = No.ToString().Length;
            if (Len == length) return No.ToString();

            int decimalLength = No.ToString("D").Length + length;
            if (Len == 1)
                decimalLength = No.ToString("D").Length + (length - Len);

            if (Len == 2)
                decimalLength = No.ToString("D").Length + (length - Len);
            if (Len == 3)
                decimalLength = No.ToString("D").Length + (length - Len);
            return (No.ToString("D" + decimalLength.ToString()));
        }

        public List<DaysViewModel> MonthDaysList(int month)
        {
            int year = Convert.ToInt32(DateTime.Now.Year.ToString("0#"));

            var days = DateTime.DaysInMonth(year, month);
            List<DaysViewModel> MonthDays = new List<DaysViewModel>();

            for (int i = 1; i <= days; i++)
            {
                DaysViewModel day = new DaysViewModel();
                day.DOC = i;
                MonthDays.Add(day);
            }

            return MonthDays;
        }

        public string GetRandomLatLong()
        {
            string valid = ("1234567890");
            int lat = 2;
            int decimalvalue = 6;
            Random random = new Random();
            StringBuilder strA = new StringBuilder(100);

            while (lat-- > 0)
            {
                strA.Append(valid[random.Next(valid.Length)]);
            }

            StringBuilder strB = new StringBuilder(100);
            while (decimalvalue-- > 0)
            {
                strB.Append(valid[random.Next(valid.Length)]);
            }
            string latlong = strA + "." + strB;

            return latlong.ToString();
        }

        public int GetDFOSOrSDFOSParentID(int UserID, int ParentID)
        {
            int usertypeid = new UserDAL().SelectUserTypeID(UserID);
            if (isDFOSOrSDFOS(usertypeid))
            {
                return new UserDAL().SelectParentID(UserID);
            }

            if (isRoleAdminOrSupport(usertypeid))
            {
                return ParentID;
            }
            return UserID;
        }

        public decimal PercentageForAmountWiseInsigths(decimal Amount, decimal TotalAmount, decimal SumOFTotalAmount)
        {
            if (Amount <= 0)
            {
                return 0;
            }
            decimal percentageamount = (TotalAmount / SumOFTotalAmount) * 100;

            return percentageamount.TwoDigitDecimalValue();
        }
        internal static bool IsDownload(bool IsExcelDownload,bool IsPDFDownload)
        {
            if (IsExcelDownload)
            {
                return true;
            }
            if (IsPDFDownload)
            {
                return true;
            }
            return false;
        }

        public string getEndDate(int DayCount, string endDateString)
        {
            if (!DateTime.TryParse(endDateString, out DateTime endDate))
            {
                // Handle invalid date string, return an error message, or throw an exception
                return "Invalid date format";
            }

            int additionalDays = (int)(endDate - DateTime.Now).TotalDays;

            if (additionalDays > 0)
            {
                DayCount = DayCount + additionalDays;
            }

            DateTime newEndDate = DateTime.Now.AddDays(DayCount);
            return newEndDate.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture);
        }
   
    }
}