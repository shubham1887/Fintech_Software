using Project.DAL;
using Project.Model;
using Project.Processing;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UserBalanceHistoryBAL
    {
        internal void Select()
        {
            try
            {
                List<UserBalanceHistory> list = new UserBalanceHistoryDAL().SelectForToday();

                if (list != null && list.Count >= 1)
                {
                    return;
                }

                new UserBalanceHistoryDAL().InsertBulk();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        public void BulkInsertOpeningBalance()
        {
            try
            {
                List<UserBalanceHistory> list = new UserBalanceHistoryDAL().SelectWithoutCacheForToday();

                if (list != null && list.Count >= 1)
                {
                    return;
                }

                new UserBalanceHistoryDAL().InsertBulk();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}