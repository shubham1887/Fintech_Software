using Project.Model;
using Project.Processing;
using Project.Requests.CircleWiseUserWiseOperatorOff;
using Project.Response.CircleWiseUserWiseOperatorOff;
using Project.StaticModel;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CircleWiseUserWiseOperatorOffDal
    {
        private readonly string key = "_CircleWiseUserWiseOperatorOff";

        public void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<CircleWiseUserWiseOperatorOff>>();
            cache.Remove(key);
        }

        internal void BulkInsert(List<CircleWiseUserWiseOperatorOff> list)
        {
            using (var context = new DatabaseContext())
            {
                context.CircleWiseUserWiseOperatorOff.AddRange(list);
                context.SaveChanges();
            }
        }

        public List<CircleWiseUserWiseOperatorOff> Select()
        {
            var _list = new MemoryCacheProcessing<List<CircleWiseUserWiseOperatorOff>>();

            List<CircleWiseUserWiseOperatorOff> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<CircleWiseUserWiseOperatorOff> SelectWithoutCache()
        {
            List<CircleWiseUserWiseOperatorOff> list;
            using (var context = new DatabaseContext())
            {
                list = context.CircleWiseUserWiseOperatorOff.ToList();
            }
            return list;
        }

        public CircleWiseUserWiseOperatorOff SelectByUserIDAndStateID(int UserID, int StateID, int operatorID)
        {
            List<CircleWiseUserWiseOperatorOff> mainList = Select();
            CircleWiseUserWiseOperatorOff obj = mainList.FirstOrDefault(S => S.StateID == StateID && S.UserID == UserID && S.OperatorID == operatorID);
            return obj;
        }

        internal List<ViewModel> SelectAllUserwiseCircleOffList()
        {
            List<CircleWiseUserWiseOperatorOff> list = Select();
            List<UserMaster> usersList = new UserDAL().Select();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectOperatorListByServiceID((int)ServiceCode.Mobile); ;

            List<ViewModel> listRet = (from CircleWiseUserWise in list
                                       join operators in opList on CircleWiseUserWise.OperatorID equals operators.OperatorID
                                       join user in usersList on CircleWiseUserWise.UserID equals user.UserID
                                       select new ViewModel
                                       {
                                           CircleWiseUserWiseOperatorOffID = CircleWiseUserWise.CircleWiseUserWiseOperatorOffID,
                                           UserID = CircleWiseUserWise.UserID,
                                           UserName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                                           OperatorID = CircleWiseUserWise.OperatorID,
                                           OperatorName = operators.OperatorName,
                                       }).ToList();
            return listRet;
        }

        internal List<StateMasterList> SelectStateList(int OperatorID, int UserID)
        {
            List<CircleWiseUserWiseOperatorOff> list = Select();

            List<ZoneMaster> circles = new ZoneMasterDAL().Select();

            List<StateMasterList> listRet = (from CircleWiseUserWise in list
                                             join circle in circles on CircleWiseUserWise.StateID equals circle.ZoneMasterID
                                             where CircleWiseUserWise.OperatorID == OperatorID &&
                                                   CircleWiseUserWise.UserID == UserID
                                             select new StateMasterList
                                             {
                                                 ID = CircleWiseUserWise.StateID,
                                                 Name = circle.Name,
                                             }).ToList();
            return listRet;
        }

        internal int DeleteAll()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var alldata = context.CircleWiseUserWiseOperatorOff.ToList();
                context.CircleWiseUserWiseOperatorOff.RemoveRange(alldata);
                context.SaveChanges();
            }
            return 1;
        }

        internal int DeleteByUserID(int UserID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var slectedUserID = context.CircleWiseUserWiseOperatorOff.Where(U => U.UserID == UserID);
                context.CircleWiseUserWiseOperatorOff.RemoveRange(slectedUserID);
                context.SaveChanges();
            }
            return 1;
        }
    }
}