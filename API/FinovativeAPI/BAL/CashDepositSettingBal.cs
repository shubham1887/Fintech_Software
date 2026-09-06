using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.CashDepositSetting;
using Project.Response.CashDepositSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class CashDepositSettingBal
    {
        private readonly IMapper _mapper;

        public CashDepositSettingBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        internal ViewModel SelectFingpayCashDeposit()
        {
            ViewModel settings = null;
            try
            {
                settings = new CashDepositSettingDal().SelectCreditCardByName(CashDepositCode.Fingpay);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }

        public CashDepositSettingBal()
        {
        }

        internal ViewModel SelectPaysprintCashDeposit()
        {
            ViewModel settings = null;
            try
            {
                settings = new CashDepositSettingDal().SelectCreditCardByName(CashDepositCode.PaySprint);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }

        private void DeactiveAll()
        {
            try
            {
                List<ViewModel> settings = new CashDepositSettingDal().Select();

                foreach (ViewModel item in settings)
                {
                    CashDepositSetting ObjSetting = new CashDepositSettingDal().SelectByID(item.CashDepositSettingID);
                    ObjSetting.IsActive = false;
                    new CashDepositSettingDal().Update(ObjSetting);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }

            CashDepositSetting ObjModel = null;
            try
            {
                ObjModel = new CashDepositSettingDal().SelectByID(updateRequest.CashDepositSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.MerchantID = updateRequest.MerchantID;
            ObjModel.Slat = updateRequest.Slat;
            ObjModel.Token = updateRequest.Token;
            ObjModel.PublicKey = updateRequest.PublicKey;
            ObjModel.Mode = updateRequest.Mode;
            ObjModel.MinimumAmount = updateRequest.MinimumAmount;
            ObjModel.MaximumAmount = updateRequest.MaximumAmount;

            ObjModel.IsActive = updateRequest.IsActive;

            if (ObjModel.IsActive == true)
            {
                DeactiveAll();
            }

            int rowsAffected = -1;
            try
            {
                rowsAffected = new CashDepositSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return PaymentGatewayMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new CashDepositSettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal ViewModel SelectByID(OnOffRequest mainRequest, out string failureMessage)
        {
            ViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;

            failureMessage = DMTSettingMessages.NotFound;

            CashDepositSetting settings;
            try
            {
                settings = new CashDepositSettingDal().SelectByID(mainRequest.CashDepositSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (settings == null) return viewModel;
            try
            {
                viewModel = _mapper.Map<ViewModel>(settings);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal ViewModel ActiveCashDeposit(out string failureMessage)
        {
            Response.CashDepositSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new CashDepositSettingDal().SelectActiveCashDeposit();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel == null)
            {
                failureMessage = "Cash deposit setting is not active, please contact to customer care";
                return viewModel;
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }
    }
}