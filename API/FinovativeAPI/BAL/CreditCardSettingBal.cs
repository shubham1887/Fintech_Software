using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.CreditCardSetting;
using Project.Response.CreditCardSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class CreditCardSettingBal
    {
        private readonly IMapper _mapper;

        public CreditCardSettingBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public CreditCardSettingBal()
        {
        }

        internal ViewModel SelectByName(string Name)
        {
            ViewModel settings = null;
            try
            {
                settings = new CreditCardSettingDal().SelectCreditCardByName(Name);
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
                List<ViewModel> settings = new CreditCardSettingDal().Select();

                foreach (ViewModel item in settings)
                {
                    CreditCardSetting ObjSetting = new CreditCardSettingDal().SelectByID(item.CreditCardSettingID);
                    ObjSetting.IsActive = false;
                    new CreditCardSettingDal().Update(ObjSetting);
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

            CreditCardSetting ObjModel = null;
            try
            {
                ObjModel = new CreditCardSettingDal().SelectByID(updateRequest.CreditCardSettingID);
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
            ObjModel.MinimumAmount = updateRequest.MinimumAmount;
            ObjModel.MaximumAmount = updateRequest.MaximumAmount;
            ObjModel.Mode = updateRequest.Mode;
            ObjModel.IsActive = updateRequest.IsActive;

            if (ObjModel.IsActive == true)
            {
                DeactiveAll();
            }

            int rowsAffected = -1;
            try
            {
                rowsAffected = new CreditCardSettingDal().Update(ObjModel);
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
                viewModel = new CreditCardSettingDal().Select();
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

            CreditCardSetting settings;
            try
            {
                settings = new CreditCardSettingDal().SelectByID(mainRequest.CreditCardSettingID);
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

        internal ViewModel ActiveCreditCard(out string failureMessage)
        {
            Response.CreditCardSetting.ViewModel viewModel = null;
            try
            {
                viewModel = new CreditCardSettingDal().SelectActiveCreditCard();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (viewModel == null)
            {
                failureMessage = "Credit card setting is not active, please contact to customer care";
                return viewModel;
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }
    }
}