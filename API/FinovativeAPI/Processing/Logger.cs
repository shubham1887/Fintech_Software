using Project.DAL;
using Project.Requests;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class Logger
    {
        public void Write(Exception ex, string info = "", [CallerMemberName] string callerName = "",
                        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            Task.Run(() => WriteCore(ex, info, callerName, callerFilePath, callerLineNumber));
        }

        public void WriteCore(Exception ex, string info = "", [CallerMemberName] string callerName = "",
                        [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
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

            string ErrorMessage = $"ErrorMessage : {ex.Message}, Class :{callerFilePath}, Method : {callerName}, Line Number:{callerLineNumber}, Info:{info},  Inner Message:{innerMessage}";

            LogRequest LogOBJ = new LogRequest();
            LogOBJ.Logger = "ERROR";
            LogOBJ.LogLevel = "Logger";
            LogOBJ.Thread = "Thread";
            LogOBJ.Message = ErrorMessage;
            LogOBJ.Exception = innerMessage;
            LogDAL ObjBAL = new LogDAL();
            ObjBAL.Write(LogOBJ);
        }

        public void Info(string Pagename, string FunctionName, string Message)
        {
            Task.Run(() => InfoCore(Pagename, FunctionName, Message));
        }

        private void InfoCore(string Pagename, string FunctionName, string Message)
        {
            Message = $"{Pagename} - {FunctionName} : {Message}";

            LogRequest LogOBJ = new LogRequest();
            LogOBJ.Logger = "Info";
            LogOBJ.LogLevel = "Logger";
            LogOBJ.Thread = "Thread";
            LogOBJ.Message = Message;
            LogOBJ.Exception = "";
            LogDAL ObjBAL = new LogDAL();
            ObjBAL.Write(LogOBJ);
        }
    }
}