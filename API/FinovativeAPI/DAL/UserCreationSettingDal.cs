using Project.Model;
using Project.Processing;
using Project.Response.UserCreationSetting;
using Project.StaticModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class UserCreationSettingDal
    {
        public static readonly string Key = "_UserCreationSetting";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(UserCreationSetting model)
        {
            RemoveCache();
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.UserCreationSetting.Add(model);
                context.SaveChanges();
                result = model.UserCreationSettingID;
            }
            return result;
        }

        internal int Update(UserCreationSetting model)
        {
            RemoveCache();
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.UserCreationSetting.Update(model);
                context.SaveChanges();
                result = model.UserCreationSettingID;
            }
            return result;
        }

        internal int Delete(UserCreationSetting ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.UserCreationSetting.Remove(ObjModel);
            context.SaveChanges();
            return ObjModel.UserCreationSettingID;
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        internal ViewModel SelectByUserID(int UserID)
        {
            List<ViewModel> list = Select();
            return (from item in list
                    where item.UserID == UserID
                    select item).FirstOrDefault();
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from Uc in context.UserCreationSetting
                        orderby Uc.UserCreationSettingID descending
                        select new ViewModel
                        {
                            UserCreationSettingID = Uc.UserCreationSettingID,
                            FirmName = Uc.UserMaster.FirmName.ConcatNameMobileAliasName(Uc.UserMaster.MobileNumber, Uc.UserMaster.AliasName),
                            UserID = Uc.UserID,
                            MinimumCount = Uc.MinimumCount,
                            Cost = Uc.Cost,
                            DOC = Uc.DOC.ToString(Messages.DOCLongDisplayFormat, CultureInfo.InvariantCulture),
                            DOCInt = Uc.DOCInt
                        }).ToList();
            }
        }

        internal ViewModel SelectDuplicate(int UserID)
        {
            List<ViewModel> list = Select();
            return (from item in list
                    where item.UserID == UserID
                    select item).FirstOrDefault();
        }

        internal UserCreationSetting SelectByID(int UserCreationSettingID)
        {
            using var context = new DatabaseContext();
            return context.UserCreationSetting.Find(UserCreationSettingID);
        }

        internal decimal SelectCalculatedAmountByUserID(int UserID, int Count)
        {
            List<ViewModel> list = Select();
            ViewModel viewModel = (from item in list
                                   where item.UserID == UserID
                                   select item).FirstOrDefault();
            if (viewModel == null)
                return 0;
            return viewModel.Cost * Count;
        }
    }
}