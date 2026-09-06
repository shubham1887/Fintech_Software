using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.RichTextMedia;
using Project.Response.RichTextMedia;
using Project.StaticModel;
using Projet.Response.eKYC;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Project.BAL
{
    public class TextMediaBal
    {
        internal List<TextMediaSelectResponse> Select()
        {
            List<TextMediaSelectResponse> viewModel = new List<TextMediaSelectResponse>();
            try
            {
                List<RichTextMedia> view = new TextMediaDal().Select();
                foreach (RichTextMedia item in view)
                {
                    TextMediaSelectResponse singleItem = new TextMediaSelectResponse();
                    singleItem.ID = item.ID;
                    singleItem.Content = item.Content;
                    singleItem.Key = item.Key;
                    viewModel.Add(singleItem);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
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
            RichTextMedia ObjModel = null;
            try
            {
                ObjModel = new TextMediaDal().SelectByID(updateRequest.ID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.ID = updateRequest.ID;
            ObjModel.Key = updateRequest.Key;
            ObjModel.Content = updateRequest.Content;

            int rowAffected = -1;
            try
            {
                rowAffected = new TextMediaDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowAffected <= 0)
                return TextMediaMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal TextMediaSelectResponse SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            TextMediaSelectResponse singleItem = new TextMediaSelectResponse();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return singleItem;

            failureMessage = TextMediaMessage.NotFound;

            RichTextMedia item = new TextMediaDal().SelectByID(mainRequest.ID);
            if (item == null) return singleItem;

            singleItem.ID = item.ID;
            singleItem.Content = item.Content;
            singleItem.Key = item.Key;

            failureMessage = StatusType.Ok;
            return singleItem;
        }

        public TextMediaResponse SelectDigitalCertificateByKey(KeyDataModel keyDataModel,
            SelectMediaKeyRequest Request, out string failureMessage)
        {
            TextMediaResponse viewModel = new TextMediaResponse();
            failureMessage = Request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;

            string Content = "";
            try
            {
                RichTextMedia view = new TextMediaDal().SelectByKey(Request.Key);
                Content = view.Content;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            Project.Response.InvoiceSetting.ViewModel invoice = new InvoiceSettingDal().SelectAll();
            DateResponse resDate = new eKYCBal().GetEkycDate(keyDataModel.UserID);
            UserMaster user = new UserDAL().SelectByIDFromCache(Company.AdminUserID);
          
            try
            {
                Response.User.MemberDetailResponse response = new UserDAL().SelectDetailByUserID(keyDataModel.UserID);
                viewModel.Address = response.ShopAddress;
                viewModel.AliasName = response.AliasName;
                viewModel.SessionCode = GetSessionCode(resDate.DateIntstring, response.AliasName);
                viewModel.PersonName = response.PersonName;
                viewModel.Logo = response.Logo;
                viewModel.YesBankLogo = response.YesBankLogo;

                Content = Content.Replace("[BCAName]",viewModel.PersonName)
                    .Replace("[Address]", viewModel.Address)
                    .Replace("[SessionCode]", viewModel.SessionCode)
                     .Replace("[TrainingDate]", resDate.Date)
                    .Replace("[AliasName]", viewModel.AliasName.ToUpperCase())
                     .Replace("[Logo]", viewModel.Logo)
                    .Replace("[YesBankLogo]", viewModel.YesBankLogo);

                viewModel.Content= Content;

            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }
        private string GetSessionCode(string Datestring, string AliasName)
        {
            return $"HPYMSP{Datestring}{AliasName}";
        }
    }
}