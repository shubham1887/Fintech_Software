using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.PayoutSetting;
using Project.Response.PayoutSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class PayoutSettingBal
    {
        private readonly IMapper _mapper;

        public PayoutSettingBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public PayoutSettingBal()
        {
        }

        internal ViewModel SelectPayoutByName(string Name)
        {
            ViewModel settings = null;
            try
            {
                settings = new PayoutSettingDal().SelectPayoutByName(Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
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
            PayoutSetting ObjModel = null;
            try
            {
                ObjModel = new PayoutSettingDal().SelectByID(updateRequest.PayoutSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.MerchantID = updateRequest.MerchantID;
            ObjModel.Slat = updateRequest.Slat;
            ObjModel.Token = updateRequest.Token;
            ObjModel.ActionURL = updateRequest.ActionURL;
            ObjModel.StatusCheckURL = updateRequest.StatusCheckURL;
            ObjModel.MinimumAmount = updateRequest.MinimumAmount;
            ObjModel.MaximumAmount = updateRequest.MaximumAmount;
            ObjModel.Mode = updateRequest.Mode;

            ObjModel.IsActive = updateRequest.IsActive;

            if (ObjModel.IsActive == true)
            {
                new PayoutSettingDal().DeactiveAll();
            }

            ObjModel.IsActive = updateRequest.IsActive;
            int rowsAffected = -1;
            try
            {
                rowsAffected = new PayoutSettingDal().Update(ObjModel);
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
                viewModel = new PayoutSettingDal().Select();
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

            PayoutSetting settings;
            try
            {
                settings = new PayoutSettingDal().SelectByID(mainRequest.PayoutSettingID);
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

        internal ViewModel SelectActivePayout(out string failureMessage)
        {
            Response.PayoutSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new PayoutSettingDal().SelectActivePayout();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel == null)
            {
                failureMessage = "Payout setting is not active, please contact to customer care";
                return viewModel;
            }

            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal ViewModel SelectActivePayoutByName(string Name)
        {
            ViewModel settings = null;
            try
            {
                settings = new PayoutSettingDal().SelectActivePayoutByName(Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }
    }
}