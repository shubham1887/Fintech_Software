using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.Fund;
using Project.Requests.UserwiseSaleTarget;
using Project.Response.UserwiseSaleTarget;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UserwiseSaleTargetBal
    {
        internal List<ViewModel> Select(int UserID)
        {
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new UserwiseSaleTargetDal().SelectAll(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return list;
            }

            return list;
        }

        internal string Insert(InsertRequest insertRequest, KeyDataModel keyData, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserwiseSaleTarget ObjModel = new UserwiseSaleTarget
            {
                UserID = insertRequest.UserID,
                WalletID = insertRequest.WalletID,
                TargetAmount = insertRequest.TargetAmount,
                Commission = insertRequest.Commission,
                CommissionType = insertRequest.CommissionType,
                ParentID = keyData.UserID,
                IsDelete = false
            };

            UserwiseSaleTarget model = null;
            try
            {
                model = new UserwiseSaleTargetDal().SelectByUserID(insertRequest.UserID, insertRequest.WalletID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int rowsAffected;
            if (model == null)
            {
                try
                {
                    response = UserwiseSaleTargetMessages.InsertFailure;
                    rowsAffected = new UserwiseSaleTargetDal().Insert(ObjModel);
                    if (rowsAffected > 0)
                    {
                        response = UserwiseSaleTargetMessages.InsertSuccess;
                    }
                    else
                    {
                        failureMessage = response;
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
                failureMessage = StatusType.Ok;
                return response;
            }

            response = UserwiseSaleTargetMessages.UpdateFail;
            model.TargetAmount = insertRequest.TargetAmount;
            model.Commission = insertRequest.Commission;
            model.CommissionType = insertRequest.CommissionType;
            model.WalletID = insertRequest.WalletID;

            try
            {
                rowsAffected = new UserwiseSaleTargetDal().Update(model);

                if (rowsAffected > 0)
                {
                    response = UserwiseSaleTargetMessages.UpdateSuccess;
                }
                else
                {
                    failureMessage = response;
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Delete(DeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserwiseSaleTarget ObjModel = null;
            try
            {
                ObjModel = new UserwiseSaleTargetDal().SelectByID(deleteRequest.UserwiseSaleTargetID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel == null)
                return UserwiseSaleTargetMessages.Invalidtarget;

            ObjModel.IsDelete = true;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                int rowsAffected = new UserwiseSaleTargetDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return UserwiseSaleTargetMessages.DeleteFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal SaleTargetWrapper SaleTargetSelect(SaleTargetSelectRequest selectRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            SaleTargetWrapper wrapper = new SaleTargetWrapper();
            List<SaleViewModel> response = new List<SaleViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                if (selectRequest.UserTypeID == (int)UserType.Retailer
                    || selectRequest.UserTypeID == (int)UserType.APIUser
                    || selectRequest.UserTypeID == (int)UserType.Client
                    )
                {
                    response = new UserwiseSaleTargetDal().SaleTargetSelect(selectRequest, keyDataModel.UserID);
                }
                else
                {
                    response = new UserwiseSaleTargetDal().FundSaleTargetSelect(selectRequest, keyDataModel.UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response.Count <= 0 || response == null)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.list = response;
            wrapper.TotalCount = response.Count;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal string TransferCommission(List<TransferCommissionRequest> mainRequest, KeyDataModel keyDataModel, string IPAddress, out string failureMessge)
        {
            string TargetOnSale = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.TargetOnSale), "No");
            if (!TargetOnSale.EqualsIgnoreCase("Yes"))
                return failureMessge = UserwiseSaleTargetMessages.TargetSaleOff;

            string Result = UserwiseSaleTargetMessages.UserCommTransferSuccess;
            failureMessge = UserwiseSaleTargetMessages.UserNotFound;
            if (mainRequest == null || mainRequest.Count <= 0)
                return failureMessge;

            int FailureCount = 0, SuccessCount = 0;

            foreach (TransferCommissionRequest request in mainRequest)
            {
                failureMessge = CommissionTransfer(request, keyDataModel.UserID, IPAddress);

                if (!failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    FailureCount++;

                if (failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    SuccessCount++;
            }

            failureMessge = StatusType.Ok;
            if (FailureCount <= 0 && SuccessCount > 0) return Result;

            if (SuccessCount <= 0)
            {
                Result = UserwiseSaleTargetMessages.UserCommTransferFail;
                return Result;
            }

            Result = UserwiseSaleTargetMessages.UserCommTransferSuccessFail;
            Result = Result.Replace("[FailureCount]", FailureCount.ToString())
                           .Replace("[SuccessCount]", SuccessCount.ToString());
            return Result;
        }

        internal string CommissionTransfer(TransferCommissionRequest mainRequest, int ParentID, string IPAddress)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            failureMessage = UserwiseSaleTargetMessages.DuplicatTransfer;
            if (!new TargetSaleCommissionBal().CheckDuplicateTargetSaleCommission(mainRequest, ParentID))
                return failureMessage;

            decimal CommissionOnTargetSaleAmount = SelectCommissionOnTargetSaleAmount(mainRequest);

            failureMessage = UserwiseSaleTargetMessages.UserNotFound;
            if (CommissionOnTargetSaleAmount < 0) return failureMessage;

            Requests.Fund.TransferInsertRequest ObjModel = new Requests.Fund.TransferInsertRequest
            {
                ReceiverID = mainRequest.UserID,
                WalletID = mainRequest.WalletID,
                TotalAmount = CommissionOnTargetSaleAmount,
                Amount = CommissionOnTargetSaleAmount,
                Remark = "Sale Target Commission",
                CashCollectionRemark = "Sale Target Commission",
                CashCollectionAmount = CommissionOnTargetSaleAmount,
                PayMode = "",
                BankName = "0",
            };

            try
            {
                failureMessage = new FundBal().Transfer(ObjModel, ParentID, IPAddress);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }

            InsertTargetSaleCommissionHistory(mainRequest, ObjModel, ParentID);

            failureMessage = StatusType.Ok;
            return failureMessage;
        }

        private void InsertTargetSaleCommissionHistory(TransferCommissionRequest mainRequest, TransferInsertRequest objModel, int ParentID)
        {
            int affectedrow = 0;
            TargetSaleCommission ObjModel = new TargetSaleCommission
            {
                WalletID = mainRequest.WalletID,
                SaleAmount = mainRequest.SaleAmount,
                UserID = mainRequest.UserID,
                ParentID = ParentID,
                CommissionAmount = objModel.Amount,
                Description = "Sale Target Commission History",
                SaleMonthDOC = mainRequest.SaleMonthDOC,
                Status = FundTransferType.Transfer,
            };
            try
            {
                affectedrow = new TargetSaleCommissionBal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (affectedrow <= 0)
                return;
        }

        private decimal SelectCommissionOnTargetSaleAmount(TransferCommissionRequest mainRequest)
        {
            decimal CommissionOnTargetSaleAmount = -1;
            UserwiseSaleTarget model = null;
            try
            {
                model = new UserwiseSaleTargetDal().SelectByUserID(mainRequest.UserID, mainRequest.WalletID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return CommissionOnTargetSaleAmount;
            }
            CommissionOnTargetSaleAmount = new SlabDetailBAL().GetLower(model.Commission, model.CommissionType, Convert.ToInt32(mainRequest.SaleAmount), "Commission");

            return CommissionOnTargetSaleAmount;
        }

        internal List<SaleViewModel> SaleTargetSelectUserwise(UserwiseSaleTargetSelectRequest selectRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            List<SaleViewModel> response = new List<SaleViewModel>();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            int UserTypeID = 0;
            try
            {
                UserTypeID = new UserDAL().SelectUserTypeID(keyDataModel.UserID);

                if (UserTypeID == (int)UserType.Admin)
                {
                    failureMessage = Messages.DataNotFound;
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                if (UserTypeID == (int)UserType.Retailer
                    || UserTypeID == (int)UserType.APIUser
                    || UserTypeID == (int)UserType.Client
                    )
                {
                    response = new UserwiseSaleTargetDal().SaleTargetSelectUserwise(selectRequest, keyDataModel.UserID);
                }
                else
                {
                    response = new UserwiseSaleTargetDal().FundSaleTargetSelectUserwise(selectRequest, keyDataModel.UserID);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response.Count <= 0 || response == null)
            {
                failureMessage = Messages.DataNotFound;
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }
    }
}