using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.LeadGenerationSetting;
using Project.Response.LeadGenerationSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class LeadGenerationSettingBal
    {
        private readonly IMapper _mapper;

        public LeadGenerationSettingBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public LeadGenerationSettingBal()
        {
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

            LeadGenerationSetting ObjModel = null;
            try
            {
                ObjModel = new LeadGenerationSettingDal().SelectByID(updateRequest.LeadGenerationSettingID);
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
            ObjModel.IsActive = updateRequest.IsActive;
            ObjModel.DisplayName = updateRequest.DisplayName;
            ObjModel.Sequence = updateRequest.Sequence;

            if (ObjModel.IsActive == true)
            {
                DeatciveAll(ObjModel.LeadGenerationSettingID);
            }

            int rowsAffected = -1;
            try
            {
                rowsAffected = new LeadGenerationSettingDal().Update(ObjModel);
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

        private void DeatciveAll(int LeadGenerationSettingID)
        {
            try
            {
                List<LeadGenerationSetting> settings = new LeadGenerationSettingDal().SelectActiveAccountOpeningList();
                if (settings.Count < 1)
                {
                    return;
                }

                if (settings.Count >= 2)
                {
                    bool has = settings.Any(x => x.LeadGenerationSettingID == LeadGenerationSettingID);

                    if (has == true)
                    {
                        return;
                    }
                    foreach (LeadGenerationSetting item in settings)
                    {
                        LeadGenerationSetting ObjSetting = new LeadGenerationSettingDal().SelectByID(item.LeadGenerationSettingID);
                        ObjSetting.IsActive = false;
                        new LeadGenerationSettingDal().Update(ObjSetting);
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
                viewModel = new LeadGenerationSettingDal().Select();
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

            LeadGenerationSetting settings;
            try
            {
                settings = new LeadGenerationSettingDal().SelectByID(mainRequest.LeadGenerationSettingID);
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

        internal ViewModel ActiveLeadGeneration(out string failureMessage)
        {
            ViewModel viewModel = null;
            try
            {
                viewModel = new LeadGenerationSettingDal().SelectActiveLeadGeneration();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel == null)
            {
                failureMessage = "LeadGeneration setting is not active, please contact to customer care";
                return viewModel;
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }

        public List<ActiveList> SelectActiveList(out string failureMessage)
        {
            failureMessage = AccountOpenSettingMessage.NotFound;
            List<ActiveList> list = null;
            try
            {
                list = new LeadGenerationSettingDal().SelectActiveList();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = AccountOpenSettingMessage.DeactiveUPITrasfer;
            if (list == null || list.Count <= 0) return list;

            failureMessage = StatusType.Ok;
            return list;
        }

        internal LeadGenerationSetting SelectByName(string Name)
        {
            LeadGenerationSetting settings = null;
            try
            {
                settings = new LeadGenerationSettingDal().SelectByName(Name);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }
    }
}