using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace YesBankCore.Controller
{
    internal class SORErrorProcessing
    {
        private static ReaderWriterLock locker = new ReaderWriterLock();
         
        public void saveRequest(string input, string output, string serivcename)
        {
            try
            {
                string date = DateTime.Now.ToString("dd-MMM-yyyy");

        

                string filepath = $"logs/YES_{serivcename}__{date}_Request.txt";

                try
                {
                    locker.AcquireWriterLock(int.MaxValue);

                    if (!System.IO.File.Exists(filepath))
                    {
                        System.IO.FileStream fs = System.IO.File.Create(filepath);
                        fs.Close();
                        fs.Dispose();
                    }

                    string str = Environment.NewLine + "Date : " + DateTime.Now.ToString("dd/MMMM/yyyy hh:mm:ss tt")
                        + Environment.NewLine + "Input : " + input
                        + Environment.NewLine + "Output : " + output
                        + Environment.NewLine;

                    System.IO.File.AppendAllText(filepath, str);
                }
                finally
                {
                    locker.ReleaseWriterLock();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void error(Exception ex, string info = "", [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            var st = new StackTrace(ex, true); // create the stack trace

            var query = st.GetFrames()         // get the frames
                            .Select(frame => new
                            {
                                // get the info
                                FileName = frame.GetFileName(),
                                LineNumber = frame.GetFileLineNumber(),
                                Method = frame.GetMethod().Name,
                                Class = frame.GetMethod().DeclaringType,
                            }).First();

            string innerMessage = (ex.InnerException != null)
                      ? ex.InnerException.Message
                      : "";

            string ErrorMessage = string.Format("ErrorMessage : {0}, Class :{1}, Method : {2}, Line Number:{3}, Info:{4},  Inner Message:{5}",
                ex.Message, callerFilePath, callerName, callerLineNumber, info, innerMessage);

            ErrorSave(ErrorMessage);
        }

        private void ErrorSave(string ErrorMessage)
        {
            try
            {
                string date = DateTime.Now.ToString("dd-MMM-yyyy");

                string filepath = $"logs/Error_{date}_Request.txt";

                try
                {
                    locker.AcquireWriterLock(int.MaxValue);

                    if (!System.IO.File.Exists(filepath))
                    {
                        System.IO.FileStream fs = System.IO.File.Create(filepath);
                        fs.Close();
                        fs.Dispose();
                    }
                    string str = null;
                    str = Environment.NewLine + "Date : " + DateTime.Now.ToString("dd/MMMM/yyyy hh:mm:ss tt")
                        + Environment.NewLine + "ErrorMessage : " + ErrorMessage

                        + Environment.NewLine;

                    System.IO.File.AppendAllText(filepath, str);
                }
                finally
                {
                    locker.ReleaseWriterLock();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}