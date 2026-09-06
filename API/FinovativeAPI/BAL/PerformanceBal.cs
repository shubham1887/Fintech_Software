using Newtonsoft.Json;
using Project.DAL;
using Project.Processing;
using Project.Response.Performance;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class PerformanceBal
    {
        public void Select()
        {
            try
            {
                string Url = ConsistySupportSystem.PerformanceURL;

                if (Url.Length <= 5)
                    return;

                List<PerformanceViewModel> List = new PerformanceDal().ServicewiseSaleAmount();

                if (List == null || List.Count <= 0)
                    return;

                string TxnDoc = DateTime.Now.AddDays(-1).ToString();

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };

                Parallel.ForEach(List.AsEnumerable(), parallelOptions, item =>
                {
                    item.ClientName = Company.ClientName;
                    item.TxnDoc = TxnDoc;
                    if (item.SuccessAmount <= 0 && item.SuccessCount <= 0 && item.FailCount <= 0 && item.FailAmount <= 0)
                        return;

                    string finalrequest = JsonConvert.SerializeObject(item);
                    Url = ConsistySupportSystem.PerformanceURL;
                    Url = Url.Replace("[Keydata]", "");
                    Url = Url.Replace("[Request]", finalrequest);
                    string message = HttpProcessing.CallGetMethod(Url, 30);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex); return;
            }

            return;
        }
    }
}