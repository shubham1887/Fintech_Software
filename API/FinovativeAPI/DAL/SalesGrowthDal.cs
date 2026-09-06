using Project.Model;
using Project.Response.SaleGrowth;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SalesGrowthDal
    {
        internal List<SaleGrowthViewModel> ServicewiseSaleGrowth()
        {
            int From = Convert.ToInt32(Convert.ToDateTime(DateTime.Today.AddDays(-7)).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(DateTime.Today.AddDays(-1)).ToString(Messages.DOCIntegerFomrat));

            int FromLastOfLastWeek = Convert.ToInt32(Convert.ToDateTime(DateTime.Today.AddDays(-14)).ToString(Messages.DOCIntegerFomrat));
            int ToLastOfLastWeek = Convert.ToInt32(Convert.ToDateTime(DateTime.Today.AddDays(-8)).ToString(Messages.DOCIntegerFomrat));

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                var LastWeek = (from txn in context.TransactionMaster
                                where txn.DOCInt >= From
                                && txn.DOCInt <= To
                                select new
                                {
                                    Status = txn.Status,
                                    Amount = txn.Amount,
                                    ServiceID = txn.ServiceID,
                                }).ToList();

                var LastOfLastWeek = (from txn in context.TransactionMaster
                                      where txn.DOCInt >= FromLastOfLastWeek
                                      && txn.DOCInt <= ToLastOfLastWeek
                                      select new
                                      {
                                          Status = txn.Status,
                                          Amount = txn.Amount,
                                          ServiceID = txn.ServiceID,
                                      }).ToList();

                var LastWeeklist = (from txn in LastWeek
                                    group txn by new { txn.ServiceID } into grp
                                    select new
                                    {
                                        ServiceID = grp.Key.ServiceID,
                                        SucessCount = grp.Where(q => q.Status == StatusType.Success).Count(),
                                        SucessAMT = grp.Where(q => q.Status == StatusType.Success).Sum(X => X.Amount),
                                    }).ToList();

                var LastOfLastWeeklist = (from txn in LastOfLastWeek
                                          group txn by new { txn.ServiceID } into grp
                                          select new
                                          {
                                              ServiceID = grp.Key.ServiceID,
                                              SucessCount = grp.Where(q => q.Status == StatusType.Success).Count(),
                                              SucessAMT = grp.Where(q => q.Status == StatusType.Success).Sum(X => X.Amount),
                                          }).ToList();

                List<ServiceMaster> Servicelist = new ServiceMasterDal().SelectAll();

                var response = (from Service in Servicelist
                                join lastOflastWeekdata in LastOfLastWeeklist on Service.ServiceID equals (lastOflastWeekdata.ServiceID)
                                join lastWeekdata in LastWeeklist on Service.ServiceID equals (lastWeekdata.ServiceID)

                                select new SaleGrowthViewModel
                                {
                                    ServiceName = Service.Name.ToUpper(),
                                    LastWeekAmt = lastWeekdata?.SucessAMT ?? 0,
                                    LastWeekCount = lastWeekdata?.SucessCount ?? 0,
                                    LastofLastWeekAmt = lastOflastWeekdata?.SucessAMT ?? 0,
                                    LastofLastWeekCount = lastOflastWeekdata?.SucessCount ?? 0,
                                }).ToList();

                return response;
            }
        }
    }
}