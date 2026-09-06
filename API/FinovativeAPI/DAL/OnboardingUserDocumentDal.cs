using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OnboardingUserDocumentDal
    {
        private readonly string Key = "_OnboardingUserDocument";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<Project.Response.Onboarding.OnboardingUserDocumentViewModel>>();
            cache.Remove(Key);
        }
        public int Insert(OnboardingUserDocument ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.OnboardingUserDocument.Add(ObjModel);
            context.SaveChanges();
            return ObjModel.OnboardingUserDocumentID;
        }
        internal int InsertRange(List<OnboardingUserDocument> objListModel)
        {
            using (var context = new DatabaseContext())
            {
                context.OnboardingUserDocument.AddRange(objListModel);
                return context.SaveChanges();
            }
        }
        public int Update(OnboardingUserDocument ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.OnboardingUserDocument.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.OnboardingUserDocumentID;
        }
        internal List<Project.Response.Onboarding.OnboardingUserDocumentViewModel> SelectByID(int OnboardingUserID)
        {
            List<Project.Response.Onboarding.OnboardingUserDocumentViewModel> list = Select();
            return list.Where(user => user.OnboardingUserID == OnboardingUserID).ToList();
        }
        internal List<Project.Response.Onboarding.OnboardingUserDocumentViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<Project.Response.Onboarding.OnboardingUserDocumentViewModel>>();
            List<Project.Response.Onboarding.OnboardingUserDocumentViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }
        internal OnboardingUserDocument SelectByOnboardingUserDocumentID(int OnboardingUserDocumentID)
        {
            using var context = new DatabaseContext();
            return context.OnboardingUserDocument.FirstOrDefault(user => user.OnboardingUserDocumentID == OnboardingUserDocumentID);
        }
        
        private List<Project.Response.Onboarding.OnboardingUserDocumentViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from user in context.OnboardingUserDocument
                        orderby user.OnboardingUserDocumentID descending
                        select new Project.Response.Onboarding.OnboardingUserDocumentViewModel
                        {

                            OnboardingUserDocumentID = user.OnboardingUserDocumentID,
                            //UserID = user.UserID,
                            Ref_ID = user.Ref_ID,
                            Type = user.Type,
                            //OldImage = user.OldImage.ConcatWebAPIURL(),
                            OldImage = user.OldImage.ConcatWebAPIURL(),
                            NewImage =  user.NewImage.ConcatWebAPIURL(),
                            OnboardingUserID = user.OnboardingUserID,

                        }).ToList();
            }
           
        }
    }
}
