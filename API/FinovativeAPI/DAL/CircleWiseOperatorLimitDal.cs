using Project.Model;
using Project.Processing;
using Project.Response.CirleWiseOperatorLimit;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class CircleWiseOperatorLimitDal
    {
        private readonly string key = "_CircleWiseOperatorLimit";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<CircleWiseOperatorLimit>>();
            cache.Remove(key);
        }

        internal void Insert(CircleWiseOperatorLimit model)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.CircleWiseOperatorLimit.Add(model);
                context.SaveChanges();
            }
        }

        public List<CircleWiseOperatorLimit> Select()
        {
            var _list = new MemoryCacheProcessing<List<CircleWiseOperatorLimit>>();

            List<CircleWiseOperatorLimit> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<CircleWiseOperatorLimit> SelectWithoutCache()
        {
            List<CircleWiseOperatorLimit> list;
            using (var context = new DatabaseContext())
            {
                list = context.CircleWiseOperatorLimit.ToList();
            }
            return list;
        }

        public CircleWiseOperatorLimit CheckDuplicate(int UserID, int OperatorID, int StateID)
        {
            List<CircleWiseOperatorLimit> mainList = Select();
            CircleWiseOperatorLimit obj = mainList.FirstOrDefault(S => S.UserID == UserID && S.OperatorID == OperatorID && S.StateID == StateID);
            return obj;
        }

        internal List<ViewModel> SelectAll()
        {
            List<CircleWiseOperatorLimit> list = Select();
            List<UserMaster> usersList = new UserDAL().Select();
            List<ZoneMaster> stateList = new ZoneMasterDAL().Select();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectOperatorListByServiceID((int)ServiceCode.Mobile);

            List<ViewModel> listRet = (from CircleWiseOperator in list
                                       join operators in opList on CircleWiseOperator.OperatorID equals operators.OperatorID
                                       join user in usersList on CircleWiseOperator.UserID equals user.UserID
                                       join state in stateList on CircleWiseOperator.StateID equals state.ZoneMasterID
                                       orderby user.FirmName
                                       select new ViewModel
                                       {
                                           CircleWiseOperatorLimitID = CircleWiseOperator.CircleWiseOperatorLimitID,
                                           UserID = CircleWiseOperator.UserID,
                                           UserName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName),
                                           OperatorID = CircleWiseOperator.OperatorID,
                                           OperatorName = operators.OperatorName,
                                           StateID = state.ZoneMasterID,
                                           StateName = state.Name,
                                           AmountLimit = CircleWiseOperator.AmountLimit,
                                           RemainingLimit = SelectRemainingLimit(CircleWiseOperator.UserID, CircleWiseOperator.OperatorID, CircleWiseOperator.StateID, CircleWiseOperator.AmountLimit)
                                       }).ToList();
            return listRet;
        }

        internal decimal SelectAmountLimt(int UserID, int OperatorID, int StateID)
        {
            List<CircleWiseOperatorLimit> list = Select();

            decimal obj = (from item in list
                           where item.UserID == UserID
                           && item.OperatorID == OperatorID
                           & item.StateID == StateID
                           select item.AmountLimit).FirstOrDefault();

            return obj;
        }

        private decimal SelectRemainingLimit(int UserID, int OperatorID, int StateID, decimal AmountLimit)
        {
            int From = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            int To = From;
            decimal sumAmt = 0;

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                sumAmt = (from TransactionMaster in context.TransactionMaster
                          where TransactionMaster.DOCInt >= From
                          && TransactionMaster.DOCInt <= To
                          && TransactionMaster.UserID == UserID
                          && TransactionMaster.OperatorID == OperatorID
                          && TransactionMaster.StateID == StateID
                          && (TransactionMaster.Status == StatusType.Success || TransactionMaster.Status == StatusType.Process)

                          select TransactionMaster).Sum(e => e.Amount);
            }
            decimal RemainingLimit = AmountLimit - sumAmt;
            return RemainingLimit;
        }

        internal int DeleteAll()
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var alldata = context.CircleWiseOperatorLimit.ToList();
                context.CircleWiseOperatorLimit.RemoveRange(alldata);
                context.SaveChanges();
            }
            return 1;
        }

        internal int Delete(int CircleWiseOperatorLimitID)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                CircleWiseOperatorLimit model = context.CircleWiseOperatorLimit.FirstOrDefault(U => U.CircleWiseOperatorLimitID == CircleWiseOperatorLimitID);
                context.CircleWiseOperatorLimit.Remove(model);
                context.SaveChanges();
            }
            return 1;
        }
    }
}