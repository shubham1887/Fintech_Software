using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class OperationalHistoryBal
    {
        internal void Insert(int UserID, string DocNo, string APIType)
        {
            if (UserID <= 0) return;

            OperationalHistory ObjModel = new OperationalHistory
            {
                UserID = UserID,
                APIType = APIType,
                DocNo = DocNo ?? "",
            };
            try
            {
                Task.Run(() => new OperationalHistoryDal().Insert(ObjModel));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal bool isHitLimitExceeded(int UserID, string APIType)
        {
            if (UserID <= 0) return true;

            int hitCount = 0;
            try
            {
                hitCount = new OperationalHistoryDal().SelectValidForHit(UserID, APIType);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return true;
            }

            if (APIType.EqualsIgnoreCase(Enum.GetName(typeof(ResponseType), ResponseType.BalanceCheck)))
            {
                int BalanceCheckMaxCount = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.BalanceCheckMaxCount), 5);
                if (hitCount > BalanceCheckMaxCount)
                    return true;
            }

            if (APIType.EqualsIgnoreCase(Enum.GetName(typeof(ResponseType), ResponseType.StatusCheck)))
            {
                int StatusCheckMaxCount = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.StatusCheckMaxCount), 10);
                if (hitCount > StatusCheckMaxCount)
                    return true;
            }

            return false;
        }
    }
}