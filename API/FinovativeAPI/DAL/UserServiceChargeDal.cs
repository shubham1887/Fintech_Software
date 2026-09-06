using Microsoft.EntityFrameworkCore;
using Project.Model;
using Project.Processing;
using Project.Response.UserServiceCharge;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class UserServiceChargeDal
    {
        private string Key = "_UserServiceCharge";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<UserServiceCharge>>();
            cache.Remove(Key);
        }

        internal List<UserServiceCharge> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<UserServiceCharge>>();
            List<UserServiceCharge> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<UserServiceCharge> SelectWithoutCache()
        {
            List<UserServiceCharge> list = null;
            try
            {
                using var context = new DatabaseContext();
                list = context.UserServiceCharge.ToList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        internal async Task<int> Insert(UserServiceCharge ObjModel)
        {
            using var context = new DatabaseContext();
            context.UserServiceCharge.Add(ObjModel);
            return await context.SaveChangesAsync();
        }

        internal async Task<UserServiceCharge> Select(UserServiceCharge ObjModel)
        {
            using var context = new DatabaseContext();
            return await context.UserServiceCharge.FirstOrDefaultAsync(U => U.UserID == ObjModel.UserID && U.ServiceID == ObjModel.ServiceID);
        }

        internal UserServiceCharge SelectByID(int UserID, int ServiceID)
        {
            using var context = new DatabaseContext();
            return context.UserServiceCharge.FirstOrDefault(U => U.UserID == UserID && U.ServiceID == ServiceID);
        }

        internal int Deduct(UserServiceCharge userServiceCharge)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserServiceCharge.Add(userServiceCharge);
            context.SaveChanges();
            return userServiceCharge.UserServiceChargeID;
        }

        internal List<ReportResponse> SelectForReport(Requests.UserServiceCharge.ReportRequest mainRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<UserMaster> users = new UserDAL().Select();

            List<Response.Service.ViewModel> services = new ServiceMasterDal().Select();
            List<UserServiceCharge> list = SelectAll();

            List<ReportResponse> reportList = (from us in list
                                               join u in users on us.UserID equals u.UserID
                                               // join s in services on us.ServiceID equals s.ServiceID
                                               where us.DOCInt >= FromDate && us.DOCInt <= ToDate

                                               select new ReportResponse
                                               {
                                                   UserID = us.UserID,
                                                   Charge = us.Charge,
                                                   // Service = s.ServiceName,
                                                   FirmName = u.FirmName.ConcatNameMobileAliasName(u.MobileNumber, u.AliasName),
                                                   Remark = us.Remark,
                                                   Date = us.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                                               }).ToList();

            return reportList;
        }

        internal async Task<decimal> TotalCharge(string Date)
        {
            decimal charge = 0;
            int dateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            List<UserServiceCharge> list = SelectAll();

            var chargeobj = (from servicecharge in list
                             where servicecharge.DOCInt == dateInt
                             group servicecharge by new { servicecharge.DOCInt } into grp
                             select new
                             {
                                 Charge = grp.Sum(x => x.Charge),
                             }).FirstOrDefault();

            await Task.Delay(1);

            if (chargeobj == null)
                return charge = 0;

            return charge = chargeobj.Charge;
        }

        internal async Task<decimal> TotalChargeByUserID(int UserID, string Date)
        {
            decimal charge = 0;
            int dateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));
            List<UserServiceCharge> list = SelectAll();

            var chargeobj = (from servicecharge in list
                             where servicecharge.UserID == UserID
                             where servicecharge.DOCInt == dateInt
                             group servicecharge by new { servicecharge.DOCInt } into grp
                             select new
                             {
                                 Charge = grp.Sum(x => x.Charge),
                             }).FirstOrDefault();

            await Task.Delay(1);

            if (chargeobj == null)
                return charge = 0;

            return charge = chargeobj.Charge;
        }
    }
}