using Project.Model;
using Project.Processing;
using Project.Response.InvoiceSetting;
using Project.StaticModel;
using System;
using System.Linq;

namespace Project.DAL
{
    public class InvoiceSettingDal
    {
        private readonly string key = "_InvoiceSetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<InvoiceSetting>();
            cache.Remove(key);
        }

        internal InvoiceSetting Select()
        {
            var _settingList = new MemoryCacheProcessing<InvoiceSetting>();
            InvoiceSetting SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        private InvoiceSetting SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.InvoiceSetting.FirstOrDefault();
        }

        internal int Update(InvoiceSetting ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.InvoiceSetting.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.InvoiceSettingID;
        }

        internal InvoiceSetting SelectByID(int InvoiceSettingID)
        {
            using var context = new DatabaseContext();
            return context.InvoiceSetting.Find(InvoiceSettingID);
        }

        internal int Insert(InvoiceSetting ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.InvoiceSetting.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.InvoiceSettingID;
        }

        internal ViewModel SelectAll()
        {
            ViewModel obj = null;
            using (var context = new DatabaseContext())
            {
                obj = (from setting in context.InvoiceSetting

                       select new ViewModel
                       {
                           InvoiceSettingID = setting.InvoiceSettingID,
                           LogoURL = String.Concat(Company.WebAPIURL, setting.LogoURL),
                           SignatureURL = String.Concat(Company.WebAPIURL, setting.SignatureURL),
                           ShortAddress = setting.ShortAddress,
                           State = setting.State,
                           GSTIN = setting.GSTIN,
                           IFSC = setting.IFSC,
                           AccountHolderName = setting.AccountHolderName,
                           AccountNo = setting.AccountNo,
                           AccountType = setting.AccountType,
                           Address = setting.Address,
                           FullAddress = setting.FullAddress,
                           PAN = setting.PAN,
                           CIN = setting.CIN,
                           BankName = setting.BankName,
                           BranchName = setting.BranchName,
                           CompanyName = setting.CompanyName,
                           CompanyPrefix = setting.CompanyPrefix,
                           Country = setting.Country,
                           District = setting.District,
                           Email = setting.Email,
                           Note = setting.Note,
                           PinCode = setting.PinCode,
                           WebsiteName = setting.WebsiteName,
                       }).FirstOrDefault(); ;
            }

            return obj;
        }
    }
}