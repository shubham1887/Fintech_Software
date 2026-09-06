using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;
namespace Project.DAL
{
    public class LastChangeUserDetailsDal
    {
        private readonly string Key = "_LastChangeUserDetails";

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<Project.Response.Onboarding.LastChangeUserDetailsViewModel>>();
            cache.Remove(Key);
        }

        internal List<Project.Response.Onboarding.LastChangeUserDetailsViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<Project.Response.Onboarding.LastChangeUserDetailsViewModel>>();
            List<Project.Response.Onboarding.LastChangeUserDetailsViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }


        internal List<Project.Response.Onboarding.LastChangeUserDetailsViewModel> SelectByID(int OnboardingUserID)
        {
            List<Project.Response.Onboarding.LastChangeUserDetailsViewModel> list = Select();
            list = list.Where(user => user.OnboardingUserID == OnboardingUserID).ToList();
            return list;
        }
        private List<Project.Response.Onboarding.LastChangeUserDetailsViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from user in context.LastChangeUserDetails
                        orderby user.LastChangeUserDetailsID descending
                        select new Project.Response.Onboarding.LastChangeUserDetailsViewModel
                        {
                            DOC = user.DOC.LongFormatDate(),
                            LastChangeUserDetailsID = user.LastChangeUserDetailsID,
                            OldChanges = user.OldChanges,
                            NewChanges = user.NewChanges,
                            Change = user.Change,
                            //  FirmName = user.UserMaster.FirmName,
                            OnboardingUserID = user.OnboardingUserID,

                        }).ToList();
            }
        }


        public int Insert(LastChangeUserDetails ObjModel)
        {
            RemoveCache();
            using var context = new DatabaseContext();

            context.LastChangeUserDetails.Add(ObjModel);
            context.SaveChanges();

            return ObjModel.LastChangeUserDetailsID;
        }
    }
}
