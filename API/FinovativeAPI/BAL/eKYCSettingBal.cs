using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.eKYCSetting;
using Project.Response.eKYCSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class eKYCSettingBal
    {
        private readonly IMapper _mapper;

        public eKYCSettingBal(IMapper mapper)
        {
            _mapper = mapper;
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
            eKYCSetting ObjModel = null;
            try
            {
                ObjModel = new eKYCSettingDal().SelectByID(updateRequest.eKYCSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.Slat = updateRequest.Slat;
            ObjModel.Token = updateRequest.Token;
            ObjModel.AuthKey = updateRequest.AuthKey;
            ObjModel.ChargeAmount = updateRequest.ChargeAmount;
            ObjModel.IsActive = updateRequest.IsActive;
            ObjModel.EncryptionKey = updateRequest.EncryptionKey;
            ObjModel.MerchantID = updateRequest.MerchantID;
            ObjModel.Mode = updateRequest.Mode;

            if (ObjModel.IsActive == true)
            {
                DeatciveAll(updateRequest.eKYCSettingID);
            }

            ObjModel.IsActive = updateRequest.IsActive;
            int rowsAffected = -1;
            try
            {
                rowsAffected = new eKYCSettingDal().Update(ObjModel);
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

        public eKYCSettingBal()
        {
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new eKYCSettingDal().Select();
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
            eKYCSetting settings;
            try
            {
                settings = new eKYCSettingDal().SelectByID(mainRequest.eKYCSettingID);
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

        internal ViewModel SelectActiveeKYC(int UserID, out string failureMessage)
        {
            ViewModel viewModel = new ViewModel();
            if (IsValidManualKyc(UserID))
            {
                failureMessage = "Manual kyc";
                return viewModel;
            }
            try
            {
                viewModel = new eKYCSettingDal().SelectActiveeKYC();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel == null)
            {
                failureMessage = "eKYC setting is not active, please contact to customer care";
                return viewModel;
            }

            failureMessage = StatusType.Ok;
            return viewModel;
        }
        private bool IsValidManualKyc(int UserID)
        {
            string IsManualKycAndeKycBoth = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsManualKycAndeKycBoth), "No");
            if (!IsManualKycAndeKycBoth.EqualsIgnoreCase("Yes"))
                return false;
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            if (user == null)
                return false;
            if (!user.IseKYC)
                return false;
            return true;
        }

        internal ViewModel SelectActiveeKYCByName(string Name)
        {
            ViewModel viewModel = null;
            try
            {
                viewModel = new eKYCSettingDal().SelectActiveeKYCByName(Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return viewModel;
        }
        private void DeatciveAll(int eKYCSettingID)
        {
            try
            {
                List<eKYCSetting> settings = new eKYCSettingDal().SelectActiveEKycForDeActive();
                if (settings.Count < 1)
                {
                    return;
                }

                if (settings.Count >= 2)
                {
                    bool has = settings.Any(x => x.eKYCSettingID == eKYCSettingID);

                    if (has == true)
                    {
                        return;
                    }
                    foreach (eKYCSetting item in settings)
                    {
                        eKYCSetting ObjEkycSetting = new eKYCSettingDal().SelectByID(item.eKYCSettingID);
                        ObjEkycSetting.IsActive = false;
                        new eKYCSettingDal().Update(ObjEkycSetting);
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}