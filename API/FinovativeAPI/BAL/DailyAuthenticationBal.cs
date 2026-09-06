using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Response.AEPS;
using Project.StaticModel;
using System;
using System.Collections.Generic;


namespace Project.BAL
{
    public class DailyAuthenticationBal
    {
        internal string Insert(int UserID, int ServiceID, int ProviderID, int OperatorID, string Pipe)
        {
            string failureMessage;
            DailyAuthenticationMaster ObjModel = new DailyAuthenticationMaster
            {
                UserID = UserID,
                ProviderID = ProviderID,
                ServiceID = ServiceID,
                OperatorID = OperatorID,
                Pipe = Pipe
            };
            try
            {
                int rowsAfftected = new DailyAuthenticationDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return DailyAuthenticationMessage.InsertFailure;
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

        internal List<ViewModel> SelectReportByUserID(Requests.DailyAuthentication.SelectRequest selectRequest)
        {
            List<ViewModel> viewModel = null;
            int ProviderID = getProviderID(selectRequest.ProviderName);

            try
            {
                viewModel = new DailyAuthenticationDal().SelectReportByUserID(selectRequest, ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal ViewModel SelectLast2FAByUserID(int UserID, string ProviderName)
        {
            ViewModel viewModel = new ViewModel();

            int ProviderID = getProviderID(ProviderName);
            bool IsMerchantVerify = new AEPSSettingDal().IsMerchantVerifyByAEPSName(ProviderName);
            try
            {
                viewModel = new DailyAuthenticationDal().SelectLast2FAByUserID(UserID, ProviderID);
            }
            
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if(viewModel==null)
            {
                viewModel = new ViewModel();
                viewModel.Is2FA= true;
                viewModel.IsMerchantVerify = IsMerchantVerify;
                return viewModel; 
            }
            viewModel.IsMerchantVerify = IsMerchantVerify;
            int CurrentDate = Convert.ToInt32(Convert.ToDateTime(DateTime.Now).ToString(Messages.DOCIntegerFomrat));
            int Last2FADate = Convert.ToInt32(Convert.ToDateTime(viewModel.DOCDateTime).ToString(Messages.DOCIntegerFomrat));
            if (Last2FADate < CurrentDate)
                viewModel.Is2FA = true;

            return viewModel;
        }
        private int getProviderID(string ProviderName)
        {
            if (AEPSCode.PaysprintPaytm.EqualsIgnoreCase(ProviderName))
            {
                return Company.PaysprintPaytmAEPSProviderID;
            }
            else if (AEPSCode.PaySprint.EqualsIgnoreCase(ProviderName))
            {
                return Company.PaysprintProviderID;
            }
            else if (AEPSCode.Fingpay.EqualsIgnoreCase(ProviderName))
            {
                return Company.FingpayProviderID;
            }
            else if (AEPSCode.InstantPay.EqualsIgnoreCase(ProviderName))
            {
                return Company.InstantPayPayoutProviderID;
            }
            else if (AEPSCode.CredoPay.EqualsIgnoreCase(ProviderName))
            {
                return Company.CredoPayProviderID;
            }
            else if (AEPSCode.YesBank.EqualsIgnoreCase(ProviderName))
            {
                return Company.YesBankProviderID;
            }
            return 0;
        }
    }
}