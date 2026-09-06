using Project.Model;
using Project.Processing;
using Project.Response.UserWisePlanView;
using Project.Requests.UserWisePlanView;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class UserWisePlanViewDal
    {
        public static string Key = "_UserWisePlanViewDal";

        internal int Insert(UserWisePlanView objModel)
        {
            using var context = new DatabaseContext();
            context.UserWisePlanView.Add(objModel);
            context.SaveChanges();
            return objModel.UserWisePlanViewID;
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.UserWisePlanView
                        orderby setting.UserWisePlanViewID descending
                        select new ViewModel
                        {
                            OperatorID = setting.OperatorID,
                            ServiceID = setting.ServiceID,
                            ConsumerNumber = setting.ConsumerNumber,
                            UserID = setting.UserID,
                            DOCInt = setting.DOCInt,
                            DOC = setting.DOC.LongFormatDate(),
                        }).ToList();
            }
        }

        public List<ViewModel> SelectReport(SelectRequest mainRequest)
        {
            int FromDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.FromDate).ToString(Messages.DOCIntegerFomrat));
            int ToDate = Convert.ToInt32(Convert.ToDateTime(mainRequest.ToDate).ToString(Messages.DOCIntegerFomrat));

            List<ViewModel> mainList = Select();
            List<UserMaster> userList = new UserDAL().Select();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectActiveOperatorOnly();
            List<ServiceMaster> Servicelist = new ServiceMasterDal().SelectAll();

            int MobileServiceID = (int)ServiceCode.Mobile;
            int DTHServiceID = (int)ServiceCode.Dth;

            var query = from setting in mainList
                        join user in userList on setting.UserID equals user.UserID
                        join op in opList on setting.OperatorID equals op.OperatorID
                        join service in Servicelist on setting.ServiceID equals service.ServiceID
                        where setting.DOCInt >= FromDate
                              && setting.DOCInt <= ToDate
                              && (mainRequest.UserID == 0 || setting.UserID == mainRequest.UserID)
                        select new ViewModel
                        {
                            OperatorID = setting.OperatorID,
                            ServiceID = setting.ServiceID,
                            ConsumerNumber = setting.ConsumerNumber,
                            UserID = setting.UserID,
                            OperatorName = op.OperatorName,
                            ServiceName = service.Name,
                            DOC = setting.DOC,
                        };

            var list = query.ToList();

            var mobileServiceCount = list.Count(x => x.ServiceID == MobileServiceID);
            var dthServiceCount = list.Count(x => x.ServiceID == DTHServiceID);

            foreach (var item in list)
            {
                item.MobileServiceCount = mobileServiceCount;
                item.DTHServiceCount = dthServiceCount;
            }

            return list;
        }
        
    }
}
