using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MplanCore.Processing
{
    internal class ErrorProcessing
    {
        private static ReaderWriterLock locker = new ReaderWriterLock();

        public void saveRequest(string input, string output)
        {
            Task.Run(() => saveRequestCore(input, output));
        }

        public void saveRequestCore(string input, string output)
        {
            return;
            try
            {
                string date = DateTime.Now.ToString("dd-MMM-yyyy");

                string filepath = string.Format("logs/PlanAPI_{0}_Request.txt", date);

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

        public void Write(Exception ex, string info = "", [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            return;
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

            try
            {
                string date = DateTime.Now.ToString("dd-MMM-yyyy");

                string filepath = string.Format("logs/mPlan_error_{0}_Request.txt", date);

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
                        + Environment.NewLine + "Output : " + ErrorMessage
                        + Environment.NewLine;

                    System.IO.File.AppendAllText(filepath, str);
                }
                finally
                {
                    locker.ReleaseWriterLock();
                }
            }
            catch (Exception ex1)
            {
            }
        }

    }
}