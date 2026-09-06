using AutoMapper;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.EcollectionSetting;
using Project.Response.EcollectionSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class EcollectionSettingBal
    {
        private readonly IMapper _mapper;

        public EcollectionSettingBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public EcollectionSettingBal()
        {
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            EcollectionSetting ObjModel = null;
            try
            {
                ObjModel = new EcollectionSettingDal().SelectByID(updateRequest.EcollectionSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.PrivateKeyPath = updateRequest.PrivateKeyPath;
            ObjModel.PublicKeyPath = updateRequest.PublicKeyPath;
            ObjModel.PrivateKeyPassword = updateRequest.PrivateKeyPassword;
            ObjModel.WalletID = updateRequest.WalletID;
            ObjModel.Prefix = updateRequest.Prefix;
            ObjModel.IFSCCode = updateRequest.IFSCCode;
            int rowsAffected = -1;
            try
            {
                rowsAffected = new EcollectionSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return EcollectionMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new EcollectionSettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal ViewModel SelectICICIECollection()
        {
            ViewModel settings = null;
            try
            {
                settings = new EcollectionSettingDal().SelectByName("ICICI");
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return settings;
        }

        internal string IsOnOff(OnOffRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            EcollectionSetting ObjModel = null;
            try
            {
                ObjModel = new EcollectionSettingDal().SelectByID(updateRequest.EcollectionSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;
            ObjModel.IsActive = updateRequest.IsActive;
            int rowsAffected = -1;
            try
            {
                rowsAffected = new EcollectionSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return EcollectionMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;

            failureMessage = DMTSettingMessages.NotFound;

            EcollectionSetting settings;
            try
            {
                settings = new EcollectionSettingDal().SelectByID(mainRequest.EcollectionSettingID);
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