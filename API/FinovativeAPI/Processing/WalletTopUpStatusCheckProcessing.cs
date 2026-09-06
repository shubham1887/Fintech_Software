using Project.BAL;
using Project.Model;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class WalletTopUpStatusCheckProcessing
    {
        public void StatusCheck()
        {
            try
            {
                bool isDevelopment = new Util().isDevelopment();
                if (isDevelopment == true)
                {
                    return;
                }

                string failureMessage = new Validation().ServerDownTime();

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return; // "At ServerDownTime, status check not allowed";
                }

                List<TransactionMaster> list = null;
                try
                {
                    list = null;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex); return;
                }

                if (list == null)
                    return;

                if (list.Count <= 0)
                    return;

                List<TransactionMaster> random20 = list.OrderBy(x => Guid.NewGuid()).Take(20).ToList();

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
                };
                failureMessage = "";
                Parallel.ForEach(random20.AsEnumerable(), parallelOptions, item =>
                {
                    try
                    {
                        double seconds = DateTimeExtensions.DifferenceInSeconds(item.DOC);

                        double ExpectedSecond = 180.0D; //3 min = 3 * 60

                        if (seconds <= ExpectedSecond)
                        {
                            return;
                        }
                        string faileureMessage = new TransactionBal().isValidForBulkStatusCheck(item);
                        if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                            return;

                        Requests.Transaction.SelectByIDRequest mainRequest = new Requests.Transaction.SelectByIDRequest();
                        mainRequest.ID = item.TransactionMasterID;

                        new WalletTopUpBAL().StatusCheck(mainRequest, Company.AdminUserID, out faileureMessage);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}