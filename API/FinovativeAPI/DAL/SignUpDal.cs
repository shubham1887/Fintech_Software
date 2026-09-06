using Project.Model;
using Project.Processing;
using Project.Requests.SignUp;
using Project.Response.SignUp;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SignUpDal
    {
        private readonly string key = "_SignUp";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<SignUp>>();
            cache.Remove(key);
        }

        internal List<SignUp> Select()
        {
            var _List = new MemoryCacheProcessing<List<SignUp>>();
            List<SignUp> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;

        }

        private List<SignUp> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.SignUp.ToList();
            }
        }

        public int Insert(SignUp ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.SignUp.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.SignUpID;
            }
        }

        internal int Update(SignUp ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.SignUp.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.SignUpID;
            }
        }

        internal string CheckOnbordingPending(string MobileNumber, string PANNo)
        {
            List<SignUp> list = Select();
            SignUp res = (from item in list
                          where item.MobileNumber == MobileNumber && item.PANNo == PANNo
                          && item.Status == StatusType.Process
                          select item).FirstOrDefault();
            if (res == null)
                return StatusType.Ok;

            if (res.RedirectURL == null)
                return StatusType.Ok;

            if (res.RedirectURL.Length < 5)
                return StatusType.Ok;

            return res.RedirectURL;
        }

        internal bool CheckDuplicate(string MobileNumber,string PanNo)
        {
            List<SignUp> list = Select();
            SignUp res = (from item in list
                          where (item.MobileNumber == MobileNumber || item.PANNo == PanNo)
                          && item.Status == StatusType.Success
                          select item).FirstOrDefault();
            if (res == null)
                return false;
            return true;
        }

        internal List<SignUp> SelectByOnBoarding(string MobileNumber, string PinCode)
        {
            List<SignUp> list = Select();
            return list.Where(txn => txn.MobileNumber == MobileNumber && txn.PinCode == PinCode).ToList();
        }
        internal SignUp SelectByDetailsWithProfileID(string MobileNumber, string OrderID, string ProfileID)
        {
            List<SignUp> list = Select();
            return list.Where(txn => txn.MobileNumber == MobileNumber && txn.OrderID == OrderID && txn.ProfileID == ProfileID).FirstOrDefault();
        }

        internal List<SelectReportResponse> SelectReport(SelectReportRequest mainRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<SignUp> list = Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            return (from user in list
                    where user.DOCInt >= From && user.DOCInt <= To && (string.Equals(mainRequest.Status, "All", StringComparison.OrdinalIgnoreCase)
                      || user.Status == mainRequest.Status)

                    orderby user.DOC descending
                    select new SelectReportResponse
                    {
                        DOC = user.DOC.DOCFormatDate(),
                        MobileNumber = user.MobileNumber,
                        PanNo = user.PANNo,
                        PinCode = user.PinCode,
                        ProfileID = user.ProfileID,
                        RedirectURL = user.RedirectURL,
                        ReferralCode = user.ReferralCode,
                        EmployeeID = user.EmployeeID,
                        Status = user.Status,

                    }).ToList();
        }

    }
}
