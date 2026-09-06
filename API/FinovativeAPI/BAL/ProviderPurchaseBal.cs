using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ProviderPurchase;
using Project.Response.ProviderPurchase;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class ProviderPurchaseBal
    {
        internal string Insert(InsertRequest insertRequest)
        {
            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderPurchase ObjModel = new ProviderPurchase
            {
                ProviderID = insertRequest.ProviderID,
                WalletID = insertRequest.WalletID,
                Amount = insertRequest.Amount,
                PurchaseDate = insertRequest.PurchaseDate,
                PurchaseDOCInt = Convert.ToInt32(Convert.ToDateTime(insertRequest.PurchaseDate).ToString(Messages.DOCIntegerFomrat)),
                Remark = insertRequest.Remark,
            };
            try
            {
                int rowsAfftected = new ProviderPurchaseDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return ProviderPurchaseMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            return StatusType.Ok;
        }

        internal ReportWrapper Select(SelectRequest selectRequest)
        {
            ReportWrapper wrapper = new ReportWrapper();
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new ProviderPurchaseDal().SelectFromDateToDate(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }
            if (viewModel == null || viewModel.Count <= 0) 
                return wrapper;
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;

            if (!selectRequest.IsExcelDownload)
            {
                viewModel = viewModel.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();
            }
            wrapper.list = viewModel;
            return wrapper;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderPurchase ObjModel = null;
            try
            {
                ObjModel = new ProviderPurchaseDal().SelectByID(updateRequest.ProviderPurchaseID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.Amount = updateRequest.Amount;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new ProviderPurchaseDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return ProviderPurchaseMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal string Delete(DeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ProviderPurchase ObjModel = null;
            try
            {
                ObjModel = new ProviderPurchaseDal().SelectByID(deleteRequest.ProviderPurchaseID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.IsDelete = true;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new ProviderPurchaseDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return ProviderPurchaseMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal List<DashboardViewModel> SelectForDashboard(string Date)
        {
            List<DashboardViewModel> viewModel = null;
            try
            {
                viewModel = new ProviderPurchaseDal().SelectForDashboard(Date);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            decimal RechargeBalance = 0;
            decimal Purchase = 0;
            decimal DMTBalance = 0;
            decimal Commission = 0; ;
            foreach (DashboardViewModel item in viewModel)
            {
                item.Balances = $"RCH:{Convert.ToInt32(item.RechargeBalance)}, DMT:{Convert.ToInt32(item.DMTBalance)}";
                RechargeBalance += item.RechargeBalance;
                DMTBalance += item.DMTBalance;
                Purchase += item.Purchase;
                Commission += item.Commission;
            }
            DashboardViewModel totalItem = new DashboardViewModel();
            totalItem.ProviderName = "Total";
            totalItem.Purchase = Purchase;
            totalItem.Balances = $"RCH:{Convert.ToInt32(RechargeBalance)}, DMT:{Convert.ToInt32(DMTBalance)}";
            totalItem.Commission = Commission;
            viewModel.Add(totalItem);
            return viewModel;
        }
    }
}