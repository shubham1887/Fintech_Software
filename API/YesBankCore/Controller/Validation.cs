using System.Text.RegularExpressions;

namespace YESBankCore.Controller
{
    internal class Validation
    {
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

        public bool isValidIntegerAmount(string input)
        {
            if (!IsDigitsOnly(input))
                return false;

            int intAmount = 0;
            int.TryParse(input, out intAmount);

            if (intAmount <= 0)
            {
                return false;
            }

            return true;
        }




        public bool isValidMobileNumber(string input)
        {
            if (input.Length > 10)
                return false;

            if (input.Length != 10)
                return false;

            return true;
        }

        public bool isValidCustomerAccountNumber(string input)
        {
            if (input.Length > 20)
                return false;

            if (input.Length < 7)
                return false;

            if (!IsDigitsOnly(input))
            {
                return false;
            }

            return true;
        }

        public bool IsValidString(string inStr)
        {
            if (string.IsNullOrEmpty(inStr) || string.IsNullOrWhiteSpace(inStr))
                return false;
            if (inStr.Trim().Length <= 0)
                return false;
            if (inStr == "")
                return false;
            return true;
        }

        public bool IsValidInt(int inInt)
        {
            if (inInt == null)
                return false;
            if (inInt <= 0)
                return false;
            return true;
        }

        public bool IsValidDecimal(decimal inDec)
        {
            if (inDec == null)
                return false;
            if (inDec <= 0)
                return false;
            return true;
        }

        public bool IsValidIFSC(string str)
        {
            if (str.Length != 11)
                return false;

            return true;
        }

        internal bool IsValidTransferType(string str)
        {
            if (str.Length <= 1)
                return false;

            if (str.EqualsIgnoreCase("FT"))
                return true;

            if (str.EqualsIgnoreCase("IMPS"))
                return true;

            if (str.EqualsIgnoreCase("NEFT"))
                return true;

            //if (str.EqualsIgnoreCase("RTGS" ))
            //    return true;

            return false;
        }

        public string GetValidName(string RecipientName)
        {
            string name = RecipientName;
            string pattern = @"\b(Mr|Mrs|Miss|Ms).\s";

            string result = Regex.Replace(name, pattern, "", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"[^a-zA-Z]", " ");
            result = Regex.Replace(result, @"\s+", " ");
            return result;
        }
    }
}