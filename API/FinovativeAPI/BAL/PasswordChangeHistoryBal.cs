using Project.DAL;
using Project.Model;
using Project.Processing;
using System;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class PasswordChangeHistoryBal
    {
        internal bool SelectPasswordExpiry(int UserID)
        {
            try
            {
                DateTime lastChangedDOC = DateTime.Now;
                PasswordChangeHistory ObjModel = null;
                try
                {
                    ObjModel = new PasswordChangeHistoryDal().SelectLastDate(UserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                UserMaster user = null;
                if (ObjModel == null)
                {
                    try
                    {
                        user = new UserDAL().SelectByIDFromCache(UserID);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                    if (user != null)
                    {
                        lastChangedDOC = user.DOC;
                    }
                }

                if (ObjModel != null)
                {
                    lastChangedDOC = ObjModel.DOC;
                }

                DateTime today = DateTime.Now;
                int days = (lastChangedDOC - today).Days;

                days = Math.Abs(days);

                int PasswordExpireDays = new SettingMasterBal().SelectMax_PasswordExpireDays();

                if (days >= PasswordExpireDays)
                    return false;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return true;
            }

            return true;
        }

        internal async Task<int> SelectPasswordExpiryDays(int UserID)
        {
            DateTime lastChangedDOC = DateTime.Now;

            await Task.Delay(1);

            PasswordChangeHistory ObjModel = null;
            try
            {
                ObjModel = new PasswordChangeHistoryDal().SelectLastDate(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel != null)
            {
                lastChangedDOC = ObjModel.DOC;
            }
            UserMaster user = null;
            if (ObjModel == null)
            {
                try
                {
                    user = new UserDAL().SelectByIDFromCache(UserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }

            if (user != null)
            {
                lastChangedDOC = user.DOC;
            }

            DateTime today = DateTime.Now;
            int days = (lastChangedDOC - today).Days;
            days = Math.Abs(days);

            int PasswordExpireLimit = new SettingMasterBal().SelectMax_PasswordExpireDays();
            int reaminingdays = (days - PasswordExpireLimit);
            reaminingdays = Math.Abs(reaminingdays);
            return reaminingdays;
        }
    }
}