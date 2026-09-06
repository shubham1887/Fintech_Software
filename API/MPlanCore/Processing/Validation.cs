namespace MplanCore.Processing
{
    internal class Validation
    {
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
    }
}