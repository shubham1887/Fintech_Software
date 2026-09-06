using Project.Model;
using Project.Requests;
using Project.Response.ErrorLog;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class LogDAL
    {
        internal void Write(LogRequest logOBJ)
        {
            Log ObjLog = new Log
            {
                Thread = logOBJ.Thread,
                Level = logOBJ.LogLevel,
                Logger = logOBJ.Logger,
                Message = logOBJ.Message,
                Exception = logOBJ.Exception
            };

            try
            {
                using var context = new DatabaseContext();
                context.Log.Add(ObjLog);
                context.SaveChanges();
            }
            catch (Exception)
            {
            }
        }

        public List<ViewModel> SelectForSend()

        {
            List<ViewModel> response = new List<ViewModel>();

            int Yesterday = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-1)).ToString(Messages.DOCIntegerFomrat));

            using var context = new DatabaseContext();

            var qry = (from item in context.Log
                       where item.Logger == "ERROR" && item.DOCInt == Yesterday
                       group item by new { Exception = item.Exception, Message = item.Message } into grp
                       orderby grp.Count()
                       select new
                       {
                           Exception = grp.Key.Exception,
                           Message = grp.Key.Message,
                           Cnt = grp.Count()
                       }).ToList();

            response = (from t in qry
                        where t.Cnt > 5 && t.Exception != ""
                        select new ViewModel
                        {
                            Exception = t.Exception,
                            Message = t.Message,
                        }

           ).ToList();
            return response;
        }
    }
}