using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.UserwiseSaleTarget;
using System;

namespace Project.BAL
{
    public class TargetSaleCommissionBal
    {
        internal int Insert(TargetSaleCommission insertRequest)
        {
            int rowsAffected = 0;
            try
            {
                rowsAffected = new TargetSaleCommissionDal().Insert(insertRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return rowsAffected;
        }

        internal bool CheckDuplicateTargetSaleCommission(TransferCommissionRequest mainRequest, int parentID)
        {
            TargetSaleCommission ObjModel = null;
            try
            {
                ObjModel = new TargetSaleCommissionDal().SelectByIDAndMonth(mainRequest.SaleMonthDOC, mainRequest.UserID, mainRequest.WalletID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }

            if (ObjModel == null)
                return true;

            return false;
        }
    }
}