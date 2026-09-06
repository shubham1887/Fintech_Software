using Project.Model;
using Project.Response.Performance;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class PerformanceDal
    {
        internal List<PerformanceViewModel> ServicewiseSaleAmount()
        {
            int LastDate = Convert.ToInt32(Convert.ToDateTime(DateTime.Today.AddDays(-1)).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                var LastDay = (from txn in context.TransactionMaster
                               where txn.DOCInt == LastDate
                               select new
                               {
                                   Status = txn.Status,
                                   Amount = txn.Amount,
                                   ServiceID = txn.ServiceID,
                               }).ToList();

                var Yesturdaylist = (from txn in LastDay
                                     group txn by new { txn.ServiceID } into grp
                                     select new
                                     {
                                         ServiceID = grp.Key.ServiceID,
                                         SucessCount = grp.Where(q => q.Status == StatusType.Success).Count(),
                                         SucessAMT = grp.Where(q => q.Status == StatusType.Success).Sum(X => X.Amount),
                                         FailCount = grp.Where(q => q.Status == StatusType.Failure).Count(),
                                         FailAMT = grp.Where(q => q.Status == StatusType.Failure).Sum(X => X.Amount),
                                     }).ToList();

                List<ServiceMaster> Servicelist = new ServiceMasterDal().SelectAll();

                var response = (from Service in Servicelist
                                join lastdata in Yesturdaylist on Service.ServiceID equals (lastdata.ServiceID)

                                select new PerformanceViewModel
                                {
                                    ServiceName = Service.Name.ToUpper(),
                                    SuccessAmount = lastdata?.SucessAMT ?? 0,
                                    SuccessCount = lastdata?.SucessCount ?? 0,
                                    FailAmount = lastdata?.FailAMT ?? 0,
                                    FailCount = lastdata?.FailCount ?? 0,
                                }).ToList();

                return response;
            }
        }
    }
}