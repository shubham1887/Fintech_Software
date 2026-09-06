using Project.Model;
using Project.Processing;
using Project.Response.VerificationSetting;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class VerificationSettingsDal
    {
        private readonly string key = "_VerificationSetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(key);
        }

        internal List<ViewModel> Select()
        {
            var _settingList = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> SettingList = _settingList.GetOrCreate(key, () => SelectWithoutCache());
            return SettingList;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from setting in context.VerificationSetting
                        orderby setting.VerificationSettingID descending
                        where setting.IsActiveForWebsite == true
                        select new ViewModel
                        {
                            VerificationSettingID = setting.VerificationSettingID,
                            Name = setting.Name,
                            UserName = setting.UserName,
                            AuthCode = setting.AuthCode,                         
                            Token = setting.Token,
                            CallBackURL = setting.CallBackURL,                        
                            IsActive = setting.IsActive,
                            Mode = setting.Mode,
                            Sequence = setting.Sequence,
                            IsAccountVerify = setting.IsAccountVerify,
                            IsUPIVerify = setting.IsUPIVerify,
                            IsPayoutVerify = setting.IsPayoutVerify,
                        }).ToList();
            }
        }

        internal int Update(VerificationSetting model)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.VerificationSetting.Update(model);
                return context.SaveChanges();
            }
        }

        internal List<VerificationSetting> SelectActiveSettingForDeActive()
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                return (from Verify in context.VerificationSetting
                        where Verify.IsActiveForWebsite == true && Verify.IsActive == true
                        select Verify).ToList();
            }
        }

        internal VerificationSetting SelectByID(int VerificationSettingID)
        {
            using var context = new DatabaseContext();
            return context.VerificationSetting.Find(VerificationSettingID);
        }

        internal ViewModel SelectByPrimaryID(int VerificationSettingID)
        {
            List<ViewModel> List = Select();
            ViewModel pg = List.FirstOrDefault(obj => obj.VerificationSettingID == VerificationSettingID);
            return pg;
        }

        internal ViewModel SelectByName(string name)
        {
            List<ViewModel> VerifyList = Select();
            return VerifyList.FirstOrDefault(d => d.Name.ToLower() == name.ToLower());
        }

        internal ViewModel SelectActiveVerifySetting()
        {
            List<ViewModel> VerifyList = Select();
            return VerifyList.FirstOrDefault(obj => obj.IsActive == true && obj.IsAccountVerify ==true);
        }

        internal ViewModel SelectActiveUPIVerifySetting()
        {
            List<ViewModel> VerifyList = Select();
            return VerifyList.FirstOrDefault(obj => obj.IsActive == true && obj.IsUPIVerify == true);
        }

        internal ViewModel SelectActivePayoutVerifySetting()
        {
            List<ViewModel> VerifyList = Select();
            return VerifyList.FirstOrDefault(obj => obj.IsActive == true && obj.IsPayoutVerify == true);
        }

        internal ViewModel SelectActiveList()
        {
            List<ViewModel> List = Select();
            ViewModel pg = List.FirstOrDefault(obj => obj.IsActive == true);
            return pg;
        }
    }
}
