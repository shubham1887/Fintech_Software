using DocumentFormat.OpenXml.Bibliography;
using Project.DAL;
using Project.StaticModel;
using System;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;

namespace Project.Processing
{
    public static class StringExtension
    {
        public static string CapitalizeFirstChar(this string source)
        {
            if (string.IsNullOrEmpty(source))
                return source;
          

            return char.ToUpper(source[0]) + source.Substring(1).ToLower();
        }

        public static string TrimIfNotNull(this string source)
        {
            if (source == null)
                return "";

            return source.Trim();
        }

        public static string TrimIfNotNullWithDefault(this string source, string defaultValue)
        {
            if (defaultValue == null)
                defaultValue = "";

            defaultValue = defaultValue.Trim();

            if (source == null)
                return defaultValue;

            return source.Trim();
        }

        public static string ToTitleCase(this string source)
        {
            if (source == null)
                return "";

            TextInfo txtInfo = new CultureInfo("en-US", false).TextInfo;

            return txtInfo.ToTitleCase(source.Trim().ToLower());
        }

        public static string ToUpperCase(this string source)
        {
            if (source == null)
                return "";

            return (source.Trim().ToUpper());
        }

        public static string GetGUID(this string source)
        {
            return Guid.NewGuid().ToString();
        }

        public static string GetAlphaNumbericGUID(this string source, int length)
        {
            string inStr = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            if (inStr.Length <= length)
            {
                return inStr;
            }
            return inStr.Substring(0, length - 1);
        }

        public static bool EqualsIgnoreCase(this string source, string secondString)
        {
            if (source == null)
                return false;

            if (secondString == null)
                return false;

            if (!source.Equals(secondString, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static string UrlEncode(this string source)
        {
            if (source == null)
            {
                return "";
            }

            if (source.Length <= 0)
            {
                return "";
            }
            return System.Web.HttpUtility.UrlEncode(source);
        }

        public static string ConcatNameMobileAliasName(this string source, string Mobile, string AliasName)
        {
            if (source == null)
            {
                return "";
            }
            if (Mobile == null)
                Mobile = "";

            if (new UserDAL().IsAdminByMobileNumber(Mobile))
                Mobile = "";

            if (AliasName == null)
                AliasName = "";

            AliasName = AliasName.Trim();
            string format = string.Empty;

            if (Company.NameDisplayFormat != null &&
                Company.NameDisplayFormat.Trim().Length >= 7)
            {
                format = Company.NameDisplayFormat;
                format = format.Replace("[FirmName]", source);
                format = format.Replace("[AliasName]", AliasName);
                format = format.Replace("[MobileNumber]", Mobile);
                return format.Trim();
            }

            if (Mobile == null || Mobile.Length <= 0)
            {
                format = $"{AliasName.Trim()} {source.Trim()}";
                return format.Trim();
            }
            format = $"{AliasName.Trim()} {source.Trim()} [{Mobile.Trim()}]";

            return format.Trim();
        }

        public static string ConcatNameMobileAliasNameOutstanding(this string source, string Mobile, string AliasName, decimal Outstanding)
        {
            if (source == null)
            {
                return "";
            }

            if (Mobile == null)
                Mobile = "";

            if (new UserDAL().IsAdminByMobileNumber(Mobile))
                Mobile = "";

            if (AliasName == null)
                AliasName = "";

            AliasName = AliasName.Trim();
            string format = string.Empty;

            if (Company.NameDisplayFormat != null &&
               Company.NameDisplayFormat.Trim().Length >= 7)
            {
                format = Company.NameDisplayFormat;
                format = format.Replace("[FirmName]", source);
                format = format.Replace("[AliasName]", AliasName);
                format = format.Replace("[MobileNumber]", Mobile);
                format = format + $", OS:{Outstanding.TwoDigitDecimal()}";
                return format.Trim();
            }

            if (Mobile == null || Mobile.Length <= 0)
            {
                format = $"{source.Trim()} [{AliasName.Trim()}], OS:{Outstanding.TwoDigitDecimal()}";
                return format.Trim();
            }

            format = $"{source.Trim()} [{AliasName.Trim()}] [{Mobile.Trim()}], OS:{Outstanding.TwoDigitDecimal()}";

            return format.Trim();
        }

        public static string ConcatNameAliasName(this string source, string AliasName)
        {
            if (source == null)
            {
                return "";
            }

            if (AliasName == null)
                AliasName = "";

            AliasName = AliasName.Trim();

            string format = $"{AliasName.Trim()}-{source.Trim()}";

            return format.Trim();
        }
        public static string ConcatBankNameBankAccountNumberIfscCode(this string BankName,string AccountNumber,string IFSC)
        {
            if (BankName == null)
            {
                return "";
            }
            if (AccountNumber == null)
            {
                return "";
            }
            if (IFSC == null)
            {
                return "";
            }
            return $"{BankName.Trim()}-{AccountNumber.Trim()} [{IFSC.Trim()}]";
        }

        public static string ConcatNameMobileAliasNameRole(this string source, string Mobile, string AliasName, string UserType)
        {
            if (source == null)
            {
                return "";
            }

            if (Mobile == null)
                Mobile = "";

            if (new UserDAL().IsAdminByMobileNumber(Mobile))
                Mobile = "";

            if (AliasName == null)
                AliasName = "";

            AliasName = AliasName.Trim();

            if (UserType == null)
                UserType = "";

            string format = "";

            if (Company.NameDisplayFormat != null &&
                Company.NameDisplayFormat.Trim().Length >= 7)
            {
                format = Company.NameDisplayFormat;
                format = format.Replace("[FirmName]", source);
                format = format.Replace("[AliasName]", AliasName);
                format = format.Replace("[MobileNumber]", Mobile);

                format = $"{format.Trim()} ({UserType.Trim()})";

                return format.Trim();
            }

            if (Mobile == null || Mobile.Length <= 0)
            {
                format = $"{AliasName.Trim()} {source.Trim()} ({UserType.Trim()})";
                return format.Trim();
            }

            format = $"{AliasName.Trim()} {source.Trim()} [{Mobile.Trim()}] ({UserType.Trim()})";

            return format.Trim();
        }

        public static string ConcatWebAPIURL(this string source)
        {
            if (source == null)
            {
                return "";
            }

            if (source.Length <= 2)
            {
                return "";
            }

            string prefix = Company.WebAPIURL.Trim();
            if (!Company.WebAPIURL.Trim().EndsWith("/"))
            {
                prefix = Company.WebAPIURL.Trim() + "/";
            }

             string format = $"{prefix.Trim()}{source.Trim()}";

            return format.Trim();
        }

        public static string TwoDigitDecimal(this decimal source)
        {
            return source.ToString("0.##").Trim();
        }
        
        public static string LongFormatDate(this DateTime source)
        {
            return source.ToString(StaticModel.Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture).Trim();
        }
        public static string Long24HrDisplayFormat(this DateTime source)
        {
            return source.ToString(StaticModel.Messages.DOCLong24HrDisplayFormat, CultureInfo.InvariantCulture).Trim();
        }
        public static string DOCFormatDate(this DateTime source)
        {
            return source.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture).Trim();
        }

        public static string IndiaDateFormat(this DateTime source)
        {
            return source.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture).Trim();
        }

        public static string ShortFormatDate(this DateTime source)
        {
            return source.ToString(StaticModel.Messages.DateShortFormat, CultureInfo.InvariantCulture).Trim();
        }

        public static string MaskedLastNDigit(this string source, int count = 4)
        {
            if (source == null)
                return "";

            if (count <= 0)
                return source;

            if (source.Length <= count)
                return source;

            string asterisks = new string('x', source.Length - count);

            string last = source.Substring(source.Length - count, count);

            string result = asterisks + last;

            return result;
        }

        public static string RemoveStart(this string source, string replaceString)
        {
            if (source == null)
                return "";

            if (replaceString.Length <= 0)
                return source;

            if (!source.StartsWith(replaceString))
            {
                return source;
            }

            string last = source.Substring(replaceString.Length, source.Length - replaceString.Length);

            return last;
        }

        public static decimal TwoDigitDecimalValue(this decimal source)
        {
            return Math.Round(source, 2);
            //  return Convert.ToDecimal(source.ToString("0.##").Trim());
        }

        public static string GetMACAddress()
        {
            var macAddr =
                (
                    from nic in NetworkInterface.GetAllNetworkInterfaces()
                    where nic.OperationalStatus == OperationalStatus.Up
                    select nic.GetPhysicalAddress().ToString()
                ).FirstOrDefault();

            return macAddr;
        }

    }
}