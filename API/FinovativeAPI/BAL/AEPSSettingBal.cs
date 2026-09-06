using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.AEPSSetting;
using Project.Response.AEPSSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class AEPSSettingBal
    {
        private readonly IMapper _mapper;

        public AEPSSettingBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public AEPSSettingBal()
        {
        }

        internal ViewModel SelectYesBank()
        {
            ViewModel settings = null;
            try
            {
                settings = new AEPSSettingDal().SelectAEPSByName(AEPSCode.YesBank);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }

        private void DeactiveAll(int AEPSSettingID)
        {
            try
            {
                List<AEPSSetting> settings = new AEPSSettingDal().SelectActiveAEPSForDeActive();
                if (settings.Count < 1)
                {
                    return;
                }
                if (settings.Count >= 2)
                {
                    bool has = settings.Any(x => x.AEPSSettingID == AEPSSettingID);
                    if (has == true)
                    {
                        return;
                    }

                    foreach (AEPSSetting item in settings)
                    {
                        AEPSSetting ObjSetting = new AEPSSettingDal().SelectByID(item.AEPSSettingID);
                        ObjSetting.IsActive = false;
                        new AEPSSettingDal().Update(ObjSetting);
                    }
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
            AEPSSetting ObjModel = null;
            try
            {
                ObjModel = new AEPSSettingDal().SelectByID(updateRequest.AEPSSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.AEPSSettingID = updateRequest.AEPSSettingID;
            ObjModel.MerchantID = updateRequest.MerchantID;
            ObjModel.Slat = updateRequest.Slat;
            ObjModel.Token = updateRequest.Token;
            ObjModel.PublicKey = updateRequest.PublicKey;
            ObjModel.MinimumAmount = updateRequest.MinimumAmount;
            ObjModel.MaximumAmount = updateRequest.MaximumAmount;
            ObjModel.DisplayName = updateRequest.DisplayName;
            ObjModel.Sequence = updateRequest.Sequence;
            ObjModel.Mode = updateRequest.Mode;
            ObjModel.IsActive = updateRequest.IsActive;

            if (ObjModel.IsActive == true)
            {
                DeactiveAll(ObjModel.AEPSSettingID);
            }

            int rowsAffected = -1;
            try
            {
                rowsAffected = new AEPSSettingDal().Update(ObjModel);
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
                viewModel = new AEPSSettingDal().Select();
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

            AEPSSetting settings;
            try
            {
                settings = new AEPSSettingDal().SelectByID(mainRequest.AEPSSettingID);
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