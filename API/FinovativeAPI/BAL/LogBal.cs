using Newtonsoft.Json;
using Project.DAL;
using Project.Processing;
using Project.Response.ErrorLog;
using Project.StaticModel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class LogBal
    {
        public void Select()
        {
            try
            {
                string Url = ConsistySupportSystem.BugErrorCountURL;
                if (Url.Length <= 5)
                {
                    return;
                }

                List<ViewModel> listParam = new LogDAL().SelectForSend();
                if (listParam == null || listParam.Count <= 0)
                {
                    return;
                }

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(listParam.AsEnumerable(), parallelOptions, item =>
                {
                    SupportBugModel request = new SupportBugModel();
                    request.Company = Company.ClientName;
                    request.Exception = item.Exception + " - " + item.Message;
                    string finalrequest = JsonConvert.SerializeObject(request);

                    Url = Url.Replace("[Keydata]", "");
                    Url = Url.Replace("[Request]", finalrequest);
                    string message = HttpProcessing.CallGetMethod(Url, 30);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}