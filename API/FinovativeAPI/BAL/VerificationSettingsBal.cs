using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using Project.Requests.VerificationSetting;
using Project.Response.VerificationSetting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class VerificationSettingsBal
    {
        private readonly IMapper _mapper;

        public VerificationSettingsBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public VerificationSettingsBal()
        {
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            if (Company.IsWhiteLabel.EqualsIgnoreCase("yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }
            VerificationSetting ObjModel = null;
            try
            {
                ObjModel = new VerificationSettingsDal().SelectByID(updateRequest.VerificationSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.IsActive = updateRequest.IsActive;
            ObjModel.Token = updateRequest.Token;
            ObjModel.AuthCode = updateRequest.AuthCode;
            ObjModel.CallBackURL = updateRequest.CallBackURL;
            ObjModel.UserName = updateRequest.UserName;
            ObjModel.IsAccountVerify = updateRequest.IsAccountVerify;
            ObjModel.IsUPIVerify = updateRequest.IsUPIVerify;
            ObjModel.IsPayoutVerify = updateRequest.IsPayoutVerify;
            ObjModel.Sequence = updateRequest.Sequence;
            ObjModel.Mode = updateRequest.Mode;

            if (ObjModel.IsActive == true)
            {
                DeactiveAll(ObjModel.VerificationSettingID);
            }
            int rowAffected = -1;
            try
            {
                rowAffected = new VerificationSettingsDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowAffected <= 0)
                return BusSettingMasterMessages.UpdateFail;

            return StatusType.Ok;
        }

        private void DeactiveAll(int VerificationSettingID)
        {
            try
            {
                List<VerificationSetting> settings = new VerificationSettingsDal().SelectActiveSettingForDeActive();
                if (settings.Count < 1)
                {
                    return;
                }
                if (settings.Count >= 2)
                {
                    bool has = settings.Any(x => x.VerificationSettingID == VerificationSettingID);
                    if (has == true)
                    {
                        return;
                    }

                    foreach (VerificationSetting item in settings)
                    {
                        VerificationSetting ObjSetting = new VerificationSettingsDal().SelectByID(item.VerificationSettingID);
                        ObjSetting.IsActive = false;
                        new VerificationSettingsDal().Update(ObjSetting);
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new VerificationSettingsDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;

            failureMessage = VerificationSettingMessage.NotFound;

            viewModel = new VerificationSettingsDal().SelectByPrimaryID(mainRequest.VerificationSettingID);

            if (viewModel == null) return viewModel;

            failureMessage = StatusType.Ok;
            return viewModel;
        }

        public ViewModel SelectMultiActive(out string failureMessage)
        {
            ViewModel list = null;
            try
            {
                list = new VerificationSettingsDal().SelectActiveList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (list == null)
            {
                failureMessage = "Verification setting is not active, please contact to customer care";
                return list;
            }
            failureMessage = StatusType.Ok;
            return list;
        }

        internal ViewModel SelectVerifyByName(string Name)
        {
            ViewModel settings = null;
            try
            {
                settings = new VerificationSettingsDal().SelectByName(Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }

        internal ViewModel SelectActiveVerifySetting()
        {
            ViewModel settings = null;
            try
            {
                settings = new VerificationSettingsDal().SelectActiveVerifySetting();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }

        internal ViewModel SelectActiveUPIVerifySetting()
        {
            ViewModel settings = null;
            try
            {
                settings = new VerificationSettingsDal().SelectActiveUPIVerifySetting();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }

        internal ViewModel SelectActivePayoutVerifySetting()
        {
            ViewModel settings = null;
            try
            {
                settings = new VerificationSettingsDal().SelectActivePayoutVerifySetting();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }
    }
}