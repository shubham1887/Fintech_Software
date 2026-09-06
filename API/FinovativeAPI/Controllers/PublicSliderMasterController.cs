using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Processing;
using Project.Requests;
using Project.Requests.SliderMaster;
using Project.Response;
using Project.Response.SliderMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Controllers
{
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class PublicSliderMasterController : Controller
    {
        private readonly IMapper _mapper;
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        public PublicSliderMasterController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            string failureMessage = string.Empty;
            SelectRequest mainRequest;

            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(StaticModel.Messages.ExceptionOccured);
            }
            List<ViewModel> sliders;
            try
            {
                sliders = new SliderMasterBAL(_mapper).SelectPublicSliders(mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(failureMessage);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))

                return ResponseProcess.GetFailReason(failureMessage);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(sliders));
        }
    }
}