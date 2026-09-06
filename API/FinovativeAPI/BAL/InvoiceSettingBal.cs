using AutoMapper;
using Processing.Processing;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.InvoiceSetting;
using Project.Response.InvoiceSetting;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class InvoiceSettingBal
    {
        private readonly IMapper _mapper;

        public InvoiceSettingBal(IMapper mapper)
        {
            _mapper = mapper;
        }

        public InvoiceSettingBal()
        {
        }

        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            try
            {
                mainRequest.LogoURL = new MediaProcessing().SaveImage(mainRequest.LogoImage, MediaType.Invoice_Logo, out failureMessage);
            }
            catch (Exception ex)
            {
                mainRequest.LogoURL = "";
                new Logger().Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            try
            {
                mainRequest.SignatureURL = new MediaProcessing().SaveImage(mainRequest.SignatureImage, MediaType.Invoice_Signature, out failureMessage);
            }
            catch (Exception ex)
            {
                mainRequest.SignatureURL = "";
                new Logger().Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            failureMessage = InvoiceSettingMessages.UpdateFail;
            int Result = -1;

            InvoiceSetting Obj = new InvoiceSettingDal().Select();

            string imagepath = string.IsNullOrEmpty(mainRequest.LogoURL) ? (Obj == null) ? "" : Obj.LogoURL : mainRequest.LogoURL;
            string Signatureimagepath = string.IsNullOrEmpty(mainRequest.SignatureURL) ? (Obj == null) ? "" : Obj.LogoURL : mainRequest.SignatureURL;

            InvoiceSetting ObjModel = new InvoiceSetting
            {
                CompanyName = mainRequest.CompanyName,
                InvoiceSettingID = mainRequest.InvoiceSettingID,
                LogoURL = imagepath,
                SignatureURL = Signatureimagepath,
                CompanyPrefix = mainRequest.CompanyPrefix,
                Address = mainRequest.Address,
                GSTIN = mainRequest.GSTIN,
                PAN = mainRequest.PAN,
                CIN = mainRequest.CIN,
                State = mainRequest.State,
                AccountNo = mainRequest.AccountNo,
                AccountType = mainRequest.AccountType,
                AccountHolderName = mainRequest.AccountHolderName,
                BankName = mainRequest.BankName,
                BranchName = mainRequest.BranchName,
                Email = mainRequest.Email,
                WebsiteName = mainRequest.WebsiteName,
                Note = mainRequest.Note,
                Country = mainRequest.Country,
                District = mainRequest.District,
                FullAddress = mainRequest.FullAddress,
                ShortAddress = mainRequest.ShortAddress,
                IFSC = mainRequest.IFSC,
                PinCode = mainRequest.PinCode
            };

            if (Obj == null)
            {
                try
                {
                    ObjModel.InvoiceSettingID = 0;
                    Result = new InvoiceSettingDal().Insert(ObjModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            }
            else
            {
                try
                {
                    Result = new InvoiceSettingDal().Update(ObjModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return failureMessage;
                }
            }

            if (Result <= -1) return failureMessage;

            failureMessage = StatusType.Ok;
            return InvoiceSettingMessages.UpdateSuccess;
        }

        internal ViewModel Select(out string failureMessage)
        {
            failureMessage = InvoiceSettingMessages.NotFound;

            ViewModel viewModel = null;

            try
            {
                viewModel = new InvoiceSettingDal().SelectAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            if (viewModel == null) return viewModel;

            failureMessage = StatusType.Ok;
            return viewModel;
        }
    }
}