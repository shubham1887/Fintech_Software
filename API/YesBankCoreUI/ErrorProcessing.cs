namespace YESBankUAT
{
    internal class ErrorProcessing
    {
        public void write(Exception ex, string function, string classname)
        {
            if (!System.IO.File.Exists(Application.StartupPath + "\\ErrorLog.txt"))
            {
                System.IO.FileStream fs = System.IO.File.Create(Application.StartupPath + "\\ErrorLog.txt");
                fs.Close();
                fs.Dispose();
            }
            string str = null;
            String innerMessage = (ex.InnerException != null)
                      ? ex.InnerException.Message
                      : "";
            str = Environment.NewLine + "Date : " + DateTime.Now.ToString("dd/MMMM/yyyy hh:mm:ss tt")
                + Environment.NewLine + "Error Message : " + ex.Message
                 + Environment.NewLine + "Detailed Message : " + innerMessage
                + Environment.NewLine + "In Function : " + function
                + Environment.NewLine + "In Form : " + classname
                + Environment.NewLine + Environment.NewLine;
            System.IO.File.AppendAllText(Application.StartupPath + "\\ErrorLog.txt", str);
        }

        public void saveRequest(string input, string output)
        {
            string date = DateTime.Now.ToString("dd-MMM-yyyy");
            string exepath = Application.StartupPath;
            string filepath = string.Format("{0}/Data/Request_{1}.txt", exepath, date);

            if (!System.IO.File.Exists(filepath))
            {
                System.IO.FileStream fs = System.IO.File.Create(filepath);
                fs.Close();
                fs.Dispose();
            }
            string str = null;
            str = Environment.NewLine + "Date : " + DateTime.Now.ToString("dd/MMMM/yyyy hh:mm:ss tt")
                + Environment.NewLine + "Input : " + input
                + Environment.NewLine + "Output : " + output
                + Environment.NewLine;
            System.IO.File.AppendAllText(filepath, str);
        }
    }
}