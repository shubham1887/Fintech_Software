using Project.Model;
using Project.Processing;
using Project.Response.SignUp;
using Project.Requests.SignUp;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Project.DAL
{
    public class FailedSignUpDataDal
    {
        private readonly string key = "_FailedSignUpData";
        public int Insert(FailedSignUpData ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.FailedSignUpData.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.FailedSignUpDataID;
            }
        }

        internal List<FailedSignUpData> Select()
        {
            var _List = new MemoryCacheProcessing<List<FailedSignUpData>>();
            List<FailedSignUpData> list = _List.GetOrCreate(key, () => SelectWithoutCache());
            return list;

        }

        private List<FailedSignUpData> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return context.FailedSignUpData.ToList();
            }
        }

        internal List<SelectFailSignUpResponse> SelectReport(SelectReportRequest mainRequest)
        {
            int From = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int To = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<FailedSignUpData> list = Select();
            List<UserTypeMaster> listUserType = new UserTypeDAL().Select();

            return (from user in list
                    where user.DOCInt >= From && user.DOCInt <= To 
                    orderby user.DOC descending
                    select new SelectFailSignUpResponse
                    {
                        DOC = user.DOC.DOCFormatDate(),
                        MobileNumber = user.MobileNumber,
                        PanNo = user.PANNo,
                        PinCode = user.PinCode,
                        Status = user.Status,

                    }).ToList();
        }

    }
}
