using AutoMapper;
using Processing.Processing;
using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.SliderMaster;
using Project.Response.SliderMaster;
using Project.StaticModel;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class SliderMasterBAL
    {
        private readonly IMapper _mapper;

        public SliderMasterBAL(IMapper mapper)
        {
            _mapper = mapper;
        }

        public SliderMasterBAL()
        {
        }

        public string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            if (!new Validation().IsValidFileDimensions(mainRequest.SliderImage, mainRequest.Type, out failureMessage))
            {
                return failureMessage;
            }

            failureMessage = Message.SliderMasterMessages.InsertFail;

            string imagePath = string.Empty;
            try
            {
                imagePath = new MediaProcessing().SaveImage(mainRequest.SliderImage, MediaType.Slider, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return "";
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            if (!new Validation().IsValidString(imagePath)) return "";

            SliderMaster objModel = new SliderMaster
            {
                SliderPath = imagePath,
                Description = mainRequest.Description,
                Type = mainRequest.Type
            };
            int Result;
            try
            {
                Result = new SliderMasterDal().Insert(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return Message.SliderMasterMessages.InsertSuccess;
        }

        public string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = Message.SliderMasterMessages.UpdateFail;

            SliderMaster objModel = new SliderMasterDal().SelectByID(mainRequest.SliderMasterID);

            if (objModel == null) return "";

            string imagePath = objModel.SliderPath;
            if (!mainRequest.SliderImage.Data.IsNullOrWhiteSpace())
            {
                try
                {
                    imagePath = new MediaProcessing().SaveImage(mainRequest.SliderImage, MediaType.Slider, out failureMessage);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    failureMessage = Messages.ExceptionOccured;
                    return "";
                }
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            if (imagePath.IsNullOrWhiteSpace()) return "";

            objModel.SliderPath = imagePath;
            objModel.Description = mainRequest.Description;
            objModel.Type = mainRequest.Type;

            int Result;

            try
            {
                Result = new SliderMasterDal().Update(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return Message.SliderMasterMessages.UpdateSuccess;
        }

        public List<ViewModel> Select(out string failureMessage)
        {
            failureMessage = Message.SliderMasterMessages.NotFound;

            List<ViewModel> sliders = null;
            List<SliderMaster> list;
            try
            {
                list = new SliderMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return sliders;
            }

            if (list == null || list.Count <= 0) return sliders;
            sliders = new List<ViewModel>();

            try
            {
                sliders = _mapper.Map<List<ViewModel>>(list);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return sliders;
            }

            failureMessage = StatusType.Ok;
            return sliders;
        }

        public ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel slider = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return slider;

            failureMessage = Message.SliderMasterMessages.NotFound;

            SliderMaster objModel;
            try
            {
                objModel = new SliderMasterDal().SelectByID(mainRequest.SliderMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return slider;
            }

            if (objModel == null) return slider;

            slider = new ViewModel
            {
                Description = objModel.Description,
                Type = objModel.Type,
                SliderMasterID = objModel.SliderMasterID,
                ImagePath = String.Concat(Company.WebAPIURL, objModel.SliderPath)
            };

            failureMessage = StatusType.Ok;
            return slider;
        }

        public List<ViewModel> SelectPublicSliders(SelectRequest mainRequest, out string failureMessage)
        {
            List<ViewModel> sliders = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return sliders;

            failureMessage = Message.SliderMasterMessages.NotFound;

            List<SliderMaster> list;
            try
            {
                list = new SliderMasterDal().SelectByType(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return sliders;
            }

            if (list == null || list.Count <= 0) return sliders;
            list = list.Take(10).ToList();
            sliders = new List<ViewModel>();

            try
            {
                sliders = _mapper.Map<List<ViewModel>>(list);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return sliders;
            }

            failureMessage = StatusType.Ok;
            return sliders;
        }

        public string Delete(DeleteRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = Message.SliderMasterMessages.DeleteFail;

            SliderMaster objModel = new SliderMasterDal().SelectByID(mainRequest.SliderMasterID);

            if (objModel == null) return "";
            objModel.IsDelete = true;

            int Result;

            try
            {
                Result = new SliderMasterDal().Update(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return "";
            }

            if (Result <= 0) return "";

            failureMessage = StatusType.Ok;
            return Message.SliderMasterMessages.DeleteSuccess;
        }

        public List<ViewModel> GetAppSlider(out string failureMessage)
        {
            failureMessage = Message.SliderMasterMessages.NotFound;

            List<ViewModel> sliders = null;
            List<SliderMaster> list;
            try
            {
                list = new SliderMasterDal().GetAppSlider();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return sliders;
            }

            if (list == null || list.Count <= 0) return sliders;
            sliders = new List<ViewModel>();

            try
            {
                sliders = _mapper.Map<List<ViewModel>>(list);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return sliders;
            }

            failureMessage = StatusType.Ok;
            return sliders;
        }
    }
}