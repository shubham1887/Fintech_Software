using Processing.Processing;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.ServiceWiseSliderSetting;
using Project.Response.ServiceWiseSliderSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ServiceWiseSliderSettingBal
    {
        internal List<ViewModel> Select()
        {
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new ServiceWiseSliderSettingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        internal List<ViewModel> SelectByServiceID(SelectRequest mainRequest)
        {
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                list = new ServiceWiseSliderSettingDal().SelectByServiceID(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return list;
        }

        internal ViewModel SelectSlidersByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel response = null;

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new ServiceWiseSliderSettingDal().SelectSlidersByID(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = ServiceWiseSliderSettingMessage.NotFound;
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        public string Delete(DeleteRequest SliderRequest)
        {
            string failureMessage = SliderRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ServiceWiseSliderSetting ObjModel;
            try
            {
                ObjModel = new ServiceWiseSliderSettingDal().SelectByID(SliderRequest.ServiceWiseSliderSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return Message.ServiceWiseSliderSettingMessage.NotFound;

            ObjModel.IsDelete = true;
            ObjModel.IsActive = false;
            int Result;
            try
            {
                Result = new ServiceWiseSliderSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return Message.ServiceWiseSliderSettingMessage.DeleteFailure;

            return StatusType.Ok;
        }

        public string ActiveDeactive(ActiveDeactiveRequest SliderRequest)
        {
            string failureMessage = SliderRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ServiceWiseSliderSetting ObjModel;
            try
            {
                ObjModel = new ServiceWiseSliderSettingDal().SelectByID(SliderRequest.ServiceWiseSliderSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return Message.ServiceWiseSliderSettingMessage.NotFound;

            ObjModel.IsActive = SliderRequest.IsActive;
            int Result;
            try
            {
                Result = new ServiceWiseSliderSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return Message.ServiceWiseSliderSettingMessage.UpdateFailure;

            return StatusType.Ok;
        }

        internal string Insert(SliderSettingInsertRequest mainRequest, KeyDataModel keyDataModel)
        {
            if (mainRequest.SliderImage.Size.Length <= 0 && mainRequest.SliderImage.Type.Length <= 0)
                return ServiceWiseSliderSettingMessage.InvalidDocument;

            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            if (!new Validation().IsValidFileDimensions(mainRequest.SliderImage, mainRequest.Type, out failureMessage))
            {
                return failureMessage;
            }

            ServiceMaster model = null;
            try
            {
                model = new ServiceMasterDal().SelectByID(mainRequest.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }

            if (model == null)
            {
                return failureMessage;
            }

            string path = "";

            try
            {
                path = new MediaProcessing().SaveImage(mainRequest.SliderImage, MediaType.ServiceWiseSlider, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ServiceWiseSliderSettingMessage.DocumentnotUpload;
            }

            if (string.IsNullOrEmpty(path))
            {
                return ServiceWiseSliderSettingMessage.DocumentnotUpload;
            }

            ServiceWiseSliderSetting IsDuplicate = null;
            try
            {
                IsDuplicate = new ServiceWiseSliderSettingDal().SelectDuplicate(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (IsDuplicate != null)
            {
                return ServiceWiseSliderSettingMessage.DuplicateSlider;
            }

            ServiceWiseSliderSetting ObjModel = new ServiceWiseSliderSetting
            {
                ServiceID = mainRequest.ServiceID,
                Priority = mainRequest.Priority,
                ReferenceLink = mainRequest.ReferenceLink,
                Description = mainRequest.Description,
                Type = mainRequest.Type,
                ImagePath = path,
                IsActive = true,
            };
            failureMessage = Message.ServiceWiseSliderSettingMessage.DocumentnotUpload;
            int Result = -1;
            try
            {
                Result = new ServiceWiseSliderSettingDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (Result <= 0)
            {
                return failureMessage;
            }

            return StatusType.Ok;
        }

        internal string Update(UpdateRequest mainRequest, KeyDataModel keyDataModel)
        {
            if (mainRequest.SliderImage.Size.Length <= 0 && mainRequest.SliderImage.Type.Length <= 0)
                return ServiceWiseSliderSettingMessage.InvalidDocument;

            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            if (!new Validation().IsValidFileDimensions(mainRequest.SliderImage, mainRequest.Type, out failureMessage))
            {
                return failureMessage;
            }

            ServiceMaster model = null;
            try
            {
                model = new ServiceMasterDal().SelectByID(mainRequest.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }

            if (model == null)
            {
                return failureMessage;
            }
            string path = "";

            try
            {
                path = new MediaProcessing().SaveImage(mainRequest.SliderImage, MediaType.ServiceWiseSlider, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ServiceWiseSliderSettingMessage.DocumentnotUpload;
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            if (string.IsNullOrEmpty(path))
            {
                return ServiceWiseSliderSettingMessage.DocumentnotUpload;
            }

            ServiceWiseSliderSetting ObjModel;
            try
            {
                ObjModel = new ServiceWiseSliderSettingDal().SelectByID(mainRequest.ServiceWiseSliderSettingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return Message.ServiceWiseSliderSettingMessage.NotFound;

            ObjModel.ServiceID = mainRequest.ServiceID;
            ObjModel.Priority = mainRequest.Priority;
            ObjModel.ReferenceLink = mainRequest.ReferenceLink;
            ObjModel.Description = mainRequest.Description;
            ObjModel.Type = mainRequest.Type;
            ObjModel.ImagePath = path;
            ObjModel.IsActive = true;

            int Result;
            try
            {
                Result = new ServiceWiseSliderSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return Message.ServiceWiseSliderSettingMessage.UpdateFailure;

            return StatusType.Ok;
        }

        internal bool CheckFileExists(string path)
        {
            ServiceWiseSliderSetting response = null;
            try
            {
                response = new ServiceWiseSliderSettingDal().SelectByImageURL(path);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return true;
            }

            if (response == null) return false;

            return true;
        }
    }
}