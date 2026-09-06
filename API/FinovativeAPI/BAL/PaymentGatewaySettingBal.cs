using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.PaymentGateway;
using Project.Response.PaymentGateway;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class PaymentGatewaySettingBal
    {
        private readonly IMapper _mapper;

        public PaymentGatewaySettingBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public PaymentGatewaySettingBal()
        {
        }

        private void DeactiveAll(bool IsUPI)
        {
            try
            {
                List<ViewModel> settings = new PaymentGatewaySettingDal().Select();

                foreach (ViewModel item in settings)
                {
                    PaymentGatewaySetting ObjSetting = new PaymentGatewaySettingDal().SelectByID(item.PaymentGatewaySettingID);
                    if (ObjSetting.IsUPI == true && ObjSetting.IsActive == true && IsUPI == false)
                    {
                        continue;
                    }
                    if (ObjSetting.IsUPI == false && ObjSetting.IsActive == true && IsUPI == true)
                    {
                        continue;
                    }
                    ObjSetting.IsActive = false;
                    new PaymentGatewaySettingDal().Update(ObjSetting);
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
            PaymentGatewaySetting ObjModel = null;
            try
            {
                ObjModel = new PaymentGatewaySettingDal().SelectByID(updateRequest.PaymentGatewaySettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.MerchantID = updateRequest.MerchantID;
            ObjModel.Slat = updateRequest.Slat;
            ObjModel.Token = updateRequest.Token;
            ObjModel.SuccessURL = updateRequest.SuccessURL;
            ObjModel.FailURL = updateRequest.FailURL;
            ObjModel.ActionURL = updateRequest.ActionURL;
            ObjModel.StatusCheckURL = updateRequest.StatusCheckURL;
            ObjModel.MinimumAmount = updateRequest.MinimumAmount;
            ObjModel.MaximumAmount = updateRequest.MaximumAmount;
            ObjModel.SettlementHoldInMinutes = updateRequest.SettlementHoldInMinutes;
            ObjModel.IsUPI = updateRequest.IsUPI;
            ObjModel.IsActive = updateRequest.IsActive;

            if (ObjModel.IsActive == true)
            {
                DeactiveAll(ObjModel.IsUPI);
            }

            int rowsAffected = -1;
            try
            {
                rowsAffected = new PaymentGatewaySettingDal().Update(ObjModel);
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
                viewModel = new PaymentGatewaySettingDal().Select();
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

            PaymentGatewaySetting settings;
            try
            {
                settings = new PaymentGatewaySettingDal().SelectByID(mainRequest.PaymentGatewaySettingID);
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
    }
}