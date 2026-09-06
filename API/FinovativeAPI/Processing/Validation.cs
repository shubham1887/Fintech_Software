using Project.BAL;
using Project.StaticModel;
using System;
using System.Text.RegularExpressions;

namespace Project.Processing
{
    public class Validation
    {
        public bool IsValidFundRequestString(string inStr)
        {
            if (!IsValidString(inStr))
                return false;

            if (inStr.EqualsIgnoreCase("Accept"))
                return true;

            if (inStr.EqualsIgnoreCase("Reject"))

                return true;

            return false;
        }

        public bool isValueInteger(decimal d)
        {
            bool isInt = d == (int)d;

            return isInt;
        }

        public bool IsValidPaymentTypeString(string inStr)
        {
            if (!IsValidString(inStr))
                return false;

            if (inStr.EqualsIgnoreCase("IMPS"))
                return true;
            if (inStr.EqualsIgnoreCase("NEFT"))
                return true;

            return false;
        }

        public bool IsValidAEPSTypeString(string inStr)
        {
            if (!IsValidString(inStr))
                return false;

            if (inStr.EqualsIgnoreCase("CashWithdrawal"))
                return true;
            if (inStr.EqualsIgnoreCase("BalanceEnquiry"))
                return true;

            return false;
        }

        public bool IsValidmATMTypeString(string inStr)
        {
            if (!IsValidString(inStr))
                return false;

            if (inStr.EqualsIgnoreCase("Cash Withdrawal"))
                return true;
            if (inStr.EqualsIgnoreCase("Balance Enquiry"))
                return true;

            return false;
        }

        public bool IsValidCommissionTypeString(string inStr)
        {
            if (!IsValidString(inStr))
                return false;

            if (inStr.EqualsIgnoreCase(CommissionType.Fix))
                return true;

            if (inStr.EqualsIgnoreCase(CommissionType.Flexi))
                return true;

            return false;
        }

        public bool IsValidCommissionCategory(string inStr)
        {
            if (!IsValidString(inStr))
                return false;

            if (inStr.EqualsIgnoreCase(CommissionCategory.General))
                return true;

            if (inStr.EqualsIgnoreCase(CommissionCategory.Real))
                return true;

            return false;
        }

        public bool IsValidString(string inStr)
        {
            if (string.IsNullOrEmpty(inStr) ||
                string.IsNullOrWhiteSpace(inStr))
                return false;
            if (inStr.Trim().Length <= 0)
                return false;
            if (inStr == "")
                return false;

            return true;
        }

        public bool IsValidStringWithoutZero(string inStr)
        {
            if (string.IsNullOrEmpty(inStr) ||
                string.IsNullOrWhiteSpace(inStr))
                return false;
            if (inStr.Trim().Length <= 0)
                return false;

            if (inStr == "")
                return false;

            if (inStr == "0")
                return false;

            return true;
        }

        public bool IsValidAliasNameString(string inStr)
        {
            if (string.IsNullOrEmpty(inStr) ||
                string.IsNullOrWhiteSpace(inStr))
                return true;

            if (inStr.Trim().Length >= 12)
                return false;

            return true;
        }

        public bool IsValidStringWithMinimumLength(string inStr, int length = 5)
        {
            if (string.IsNullOrEmpty(inStr) ||
                string.IsNullOrWhiteSpace(inStr))
                return false;

            if (inStr == "")
                return false;

            if (inStr.Trim().Length < length)
                return false;

            return true;
        }

        public bool IsValidDate(DateTime InputDate)
        {
            try
            {
                DateTime date = DateTime.Parse(InputDate.ToString());
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool IsValidInt(int inInt)
        {
            if (inInt <= 0)
                return false;

            return true;
        }

        public bool IsValidpositive(int inInt)
        {
            if (inInt < 0)
                return false;

            return true;
        }

        public bool IsValidDecimal(decimal inDec)
        {
            if (inDec <= -0.0000009M)
                return false;

            return true;
        }

        public bool IsValidAmoutForAEPSAndmATM(decimal inDec)
        {
            if (inDec < -0.0000009M)
                return false;

            return true;
        }

        public bool IsValidDecimalWithNigative(decimal inDec)
        {
            if (inDec <= -999.0000009M)
                return false;

            return true;
        }


        public bool IsValidMobileNumber(string str)
        {
            int DMTSenderNumberLength = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.DMTSenderNumberLength), 10);

            if (!IsValidString(str))
                return false;

            if(str.Length > DMTSenderNumberLength)
                return false;

            if (str.Length < 10)
                return false;

            if (!IsDigitsOnly(str))
                return false;

            return true;
        }

        public bool IsDigitsOnly(string str)
        {
            if (!IsValidString(str))
                return false;

            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public bool IsValidBalance(string str)
        {
            if (!IsValidString(str))
                return false;

            return decimal.TryParse(str, out decimal n);
        }

        public bool IsValidIFSC(string str)
        {
            if (str == "00000000000")// For app Zplus DMT
                return true;
            if (!IsValidString(str)) return false;
            if (str.Length != 11) return false;
            Regex regex = new Regex(@"^[A-Z]{4}0[A-Z0-9]{6}$");
            var match = regex.Match(str);
            if (!match.Success) return false;
            return true;
        }

        public bool IsValidAccountNumber(string input)
        {
            if (!IsValidString(input)) return false;

            if (input.Length <= 6) return false;

            if (input.Length >= 22) return false;

            return true;
        }

        public bool IsValidCardNumber(string input)
        {
            return IsValidString(input) && input.Length <= 16;
        }

        public bool IsValidFromToAmount(decimal FromAmount, decimal ToAmount)
        {
            return FromAmount < ToAmount;
        }

        public bool IsValidUPIID(string input)
        {
            if (!IsValidString(input)) return false;

            Regex regex = new Regex(@"[a-zA-Z0-9.\-_]{2,256}@[a-zA-Z]{2,64}");
            Match match = regex.Match(input);
            if (!match.Success) return false;
            return true;
        }

        public bool IsValidName(string input)
        {
            if (!IsValidString(input)) return false;

            Regex regex = new Regex(@"^[\p{L} \.\-]+$");
            Match match = regex.Match(input);
            if (!match.Success) return false;
            return true;
        }

        public bool IsValidEmail(string input)
        {
            if (!IsValidString(input)) return false;

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(input);
            if (!match.Success) return false;
            return true;
        }

        public bool IsValidPanCard(string input)
        {
            if (input.Length <= 0 || input == "") return true;

            if (!IsValidString(input)) return false;

            Regex regex = new Regex(@"[A-Z]{5}[0-9]{4}[A-Z]{1}");
            Match match = regex.Match(input);
            if (!match.Success) return false;
            return true;
        }

        public bool IsValidAadharNo(string input)
        {
            if (input == "" || input.Length <= 0) return true;

            if (!IsValidString(input)) return false;

            Regex regex = new Regex(@"[2-9]{1}[0-9]{11}");
            Match match = regex.Match(input);
            if (!match.Success) return false;

            bool isValidnumber = AadharNoValidation.validateVerhoeff(input);

            return isValidnumber;
        }

        public bool IsComplexPassword(string Password)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMinimum8Chars = new Regex(@".{6,}");
            var hasLowerChar = new Regex(@"[a-z]+");

            var isValidated = hasNumber.IsMatch(Password) && hasUpperChar.IsMatch(Password) && hasMinimum8Chars.IsMatch(Password) && hasLowerChar.IsMatch(Password);

            if (isValidated == false) return false;

            return true;
        }

        public bool IsValidIP(string IP)
        {
            var hasNumber = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            var isValidated = hasNumber.IsMatch(IP);

            if (isValidated == false)
                return false;

            return true;
        }

        public bool IsValidFileOrImageSize(string input)
        {
            int InputFileSize = Convert.ToInt32(input);

            if (InputFileSize > 5145728)// 5145728  5mb      for 1.5mb   1572864
                return false;

            return true;
        }

        public bool IsValidFileSize(string input)
        {
            if (input == null) return false;

            if (!IsValidString(input)) return false;

            int InputFileSize = input.Length;
            if (InputFileSize > 5145728)// 5145728  5mb      for 1.5mb   1572864
                return false;

            return true;
        }

        public bool IsValidFileOrImageSizeForEko(string input)
        {
            if (input == null) return false;

            int InputFileSize = input.Length;
            if (InputFileSize > 1000000)// 1000000  1mb
                return false;

            return true;
        }

        public bool IsValidImageType(string input)
        {
            if (input.EqualsIgnoreCase("image/png"))
                return true;

            if (input.EqualsIgnoreCase("image/jpg"))
                return true;

            if (input.EqualsIgnoreCase("image/jpeg"))
                return true;

            //if (input.EqualsIgnoreCase("application/pdf"))
            //    return true;

            return false;
        }

        public bool IsValidFileDimensions(CustomFile file, string Type, out string failureMessage)
        {
            failureMessage = Message.SliderMasterMessages.InvalidImage;
            string content = file.Data;

            if (content == null) return false;

            if (content.Trim().Length <= 0)
                return false;

            try
            {
                var imageDataByteArray = Convert.FromBase64String(content.Replace("data:image/jpeg;base64,", ""));
                System.IO.MemoryStream ms = new System.IO.MemoryStream(imageDataByteArray, 0, imageDataByteArray.Length);
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true, false);

                var Height = image.Height;
                var Width = image.Width;

                if (Type.EqualsIgnoreCase(PlatformCode.Web))
                {
                    if (Width > 900)
                    {
                        failureMessage = Message.SliderMasterMessages.InvalidDimensions;
                        return false;
                    }
                    if (Height > 600)
                    {
                        failureMessage = Message.SliderMasterMessages.InvalidDimensions;
                        return false;
                    }
                }

                if (Type.EqualsIgnoreCase(PlatformCode.App))
                {
                    if (Width > 800)
                    {
                        failureMessage = Message.SliderMasterMessages.InvalidDimensionsApp;
                        return false;
                    }
                    if (Height > 300)
                    {
                        failureMessage = Message.SliderMasterMessages.InvalidDimensionsApp;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Message.SliderMasterMessages.Invalidfile;
                return false;
            }
            failureMessage = StatusType.Ok;
            return true;
        }

        public bool IsValidVirtualBalance(decimal Amount)
        {
            if (Amount > 500000001M)
                return false;

            return true;
        }

        public string ServerDownTime(int serviceID = 0)
        {
            int DowntimeInMinutes = 10;

            if (serviceID == (int)ServiceCode.Mobile)
            {
                DowntimeInMinutes = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.DowntimeInMinutes), 10);
            }

            if (DowntimeInMinutes <= 2)
            {
                DowntimeInMinutes = 2;
            }

            if (DowntimeInMinutes >= 15)
            {
                DowntimeInMinutes = 15;
            }

            int startMinute = 60 - DowntimeInMinutes;

            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan start = new TimeSpan(23, startMinute, 0); //11.50 PM  clock night 
            TimeSpan end = new TimeSpan(00, DowntimeInMinutes, 0); //12.10 AM

            if (start.Ticks > now.Ticks && end.Ticks < now.Ticks)
            {
                return StatusType.Ok;
            }

            return "Server updating, please try after some time";
        }

        //mostly bank down at night
        public string BankDownTime()
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan start = new TimeSpan(23, 50, 0); //11.50 PM  clock night
            TimeSpan end = new TimeSpan(04, 05, 0); //04.05 AM 6 o'clock Morning

            if (start.Ticks > now.Ticks && end.Ticks < now.Ticks)
            {
                return StatusType.Ok;
            }

            return "Server updating, please try after some time";
        }

        public string ServerDownTimeForFileUpload()
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan start = new TimeSpan(23, 30, 0); //11.30 PM  clock night
            TimeSpan end = new TimeSpan(06, 30, 0); // 6:30AM o'clock Morning

            if (start.Ticks > now.Ticks && end.Ticks < now.Ticks)
            {
                return StatusType.Ok;
            }

            return "File upload stopped during server updating, please try after 6:30 AM";
        }

        public string isValidFromAndToDate(string FromDate, string ToDate, int expectedDifference = 31)
        {
            DateTime FromDateInt = Convert.ToDateTime(FromDate);
            DateTime ToDateInt = Convert.ToDateTime(ToDate);

            if (FromDateInt > ToDateInt)
            {
                return Messages.InvalidFromDateTodate;
            }

            double diff = ToDateInt.Subtract(FromDateInt).TotalDays;

            if (diff > expectedDifference)
            {
                return Messages.Only31DaysDataAllowed;
            }

            return StatusType.Ok;
        }
    }
}