using Project.Model;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class SmsTransactionMasterDal
    {
        internal int Insert(SmsTransactionMaster model)
        {
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.SmsTransactionMaster.Add(model);
                context.SaveChanges();
                result = model.SmsTransactionMasterID;
            }
            return result;
        }

        internal int Update(SmsTransactionMaster model)
        {
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.SmsTransactionMaster.Update(model);
                context.SaveChanges();
                result = model.SmsTransactionMasterID;
            }
            return result;
        }

        internal int UpdateAll(string billNumber)
        {
            List<SmsTransactionMaster> list = null;
            using (var context = new DatabaseContext())
            {
                list = context.SmsTransactionMaster.Where(n => n.BillNumber == billNumber).ToList();
                list.ForEach(a => a.IsSuccess = true);
            }
            int Result = -1;
            if (list == null)
                return Result;
            try
            {
                using var context = new DatabaseContext();
                context.SmsTransactionMaster.UpdateRange(list);
                Result = context.SaveChanges();
            }
            catch (Exception)
            {
                return Result;
            }

            return Result;
        }

        internal SmsTransactionMaster SelectByID(int SmsTransactionMasterID)
        {
            using var context = new DatabaseContext();
            return context.SmsTransactionMaster.Find(SmsTransactionMasterID);
        }

        internal List<SmsTransactionMaster> SelectPending()
        {
            int TodaysDateInt = Convert.ToInt32(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            using var context = new DatabaseContext();
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                return (from item in context.SmsTransactionMaster
                        where item.IsSuccess == false
                        && item.DOCInt == TodaysDateInt
                        select item).ToList();
            }
        }

        internal bool IsSuccessAllSelectByBillNumber(string billNumber)
        {
            using var context = new DatabaseContext();
            List<SmsTransactionMaster> list = context.SmsTransactionMaster.Where(x => x.BillNumber == billNumber).ToList();
            if (list.Count <= 0)
                return false;
            foreach (SmsTransactionMaster item in list)
            {
                if (item.IsSuccess)
                    return false;
            }
            return true;
        }

        internal bool IsSuccessSelectByTransactionMasterID(int TransactionMasterID)
        {
            using var context = new DatabaseContext();
            {
                SmsTransactionMaster res = (from item in context.SmsTransactionMaster
                                            where item.TransactionMasterID == TransactionMasterID
                                            select item).FirstOrDefault();
                if (res == null)
                    return false;

                if (res.IsSuccess)
                    return false;
            }

            return true;
        }

        internal bool IsProcessSelectByTransactionMasterID(int TransactionMasterID)
        {
            using var context = new DatabaseContext();
            {
                SmsTransactionMaster res = (from item in context.SmsTransactionMaster
                                            where item.TransactionMasterID == TransactionMasterID
                                              && item.IsSuccess == false
                                            select item).FirstOrDefault();
                if (res == null)
                    return false;

                if (res.IsProcess)
                    return false;
            }
            return true;
        }
    }
}