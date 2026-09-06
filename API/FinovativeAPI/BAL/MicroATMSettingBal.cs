using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.MicroATMSetting;
using Project.Response.MicroATMSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class MicroATMSettingBal
    {
        private readonly IMapper _mapper;

        public MicroATMSettingBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        private void DeactiveAll(int MicroATMSettingID)
        {
            try
            {
                List<MicroATMSetting> settings = new MicroATMSettingDal().SelectActiveMicroATMForDeActive();
                if (settings.Count < 1)
                {
                    return;
                }
                if (settings.Count >= 2)
                {
                    bool has = settings.Any(x => x.MicroATMSettingID == MicroATMSettingID);
                    if (has == true)
                    {
                        return;
                    }
                    foreach (MicroATMSetting item in settings)
                    {
                        MicroATMSetting ObjSetting = new MicroATMSettingDal().SelectByID(item.MicroATMSettingID);
                        ObjSetting.IsActive = false;
                        new MicroATMSettingDal().Update(ObjSetting);
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

         

        public MicroATMSettingBal()
        {
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            MicroATMSetting ObjModel = null;
            try
            {
                ObjModel = new MicroATMSettingDal().SelectByID(updateRequest.MicroATMSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;
            ObjModel.MicroATMSettingID = updateRequest.MicroATMSettingID;
            ObjModel.Name = updateRequest.Name;
            ObjModel.DisplayName = updateRequest.DisplayName;
            ObjModel.Sequence = updateRequest.Sequence;
            ObjModel.MerchantID = updateRequest.MerchantID;
            ObjModel.Slat = updateRequest.Slat;
            ObjModel.Token = updateRequest.Token;
            ObjModel.PublicKey = updateRequest.PublicKey;
            ObjModel.MinimumAmount = updateRequest.MinimumAmount;
            ObjModel.MaximumAmount = updateRequest.MaximumAmount;
            ObjModel.UserName = updateRequest.UserName;
            ObjModel.Password = updateRequest.Password;
            ObjModel.IsActive = updateRequest.IsActive;

            if (ObjModel.IsActive == true)
            {
                DeactiveAll(ObjModel.MicroATMSettingID);
            }

            int rowsAffected = -1;
            try
            {
                rowsAffected = new MicroATMSettingDal().Update(ObjModel);
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
                viewModel = new MicroATMSettingDal().Select();
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
            ViewModel viewModel = new ViewModel();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;

            failureMessage = DMTSettingMessages.NotFound;

            MicroATMSetting settings;
            try
            {
                settings = new MicroATMSettingDal().SelectByID(mainRequest.MicroATMSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (settings == null) return viewModel;

            viewModel.MicroATMSettingID = settings.MicroATMSettingID;
            viewModel.Name = settings.Name;

            viewModel.UserName = settings.UserName;
            viewModel.Password = settings.Password;

            viewModel.MerchantID = settings.MerchantID;
            viewModel.Token = settings.Token;
            viewModel.Slat = settings.Slat;
            viewModel.IsActive = settings.IsActive;
            viewModel.MinimumAmount = settings.MinimumAmount;
            viewModel.MaximumAmount = settings.MaximumAmount;
            viewModel.DisplayName = settings.DisplayName;
            viewModel.Sequence = settings.Sequence;

            viewModel.PublicKey = settings.PublicKey;

            failureMessage = StatusType.Ok;
            return viewModel;
        }

      
        internal ViewModel SelectPaySwiffMicroATM()
        {
            ViewModel settings = null;
            try
            {
                settings = new MicroATMSettingDal().SelectMicroATMByName(MicroATMCode.PaySwiff);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }
    }
}