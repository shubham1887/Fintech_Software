using Project.Model;
using Project.Processing;
using Project.Response.Onboarding;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Project.DAL
{
    public class LOB_DetailsDal
    {
        private readonly string Key = "_OnboardingUser";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<Model.LOB_Details>>();
            cache.Remove(Key);
        }
        internal List<Model.LOB_Details> Select()
        {
            var _list = new MemoryCacheProcessing<List<Model.LOB_Details>>();
            List<Model.LOB_Details> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }
        internal List<Model.LOB_Details> SelectWithoutCache()
        {

            using (var context = new DatabaseContext())
            {
                return (from LOB in context.LOB_Details
                        orderby LOB.LOB_MasterID descending
                        select new Model.LOB_Details
                        {
                            LOB_MasterID = LOB.LOB_MasterID,
                            TypeOfShop = LOB.TypeOfShop,
                            Description = LOB.Description,
                            LineOfBusiness = LOB.LineOfBusiness,
                            LOBCode = LOB.LOBCode
                        }).ToList();
            }

        }
        internal List<Model.LOB_Details> SelectReport()
        {
            List<Model.LOB_Details> list = Select();
            list = (from LOB in list

                    select new Model.LOB_Details
                    {
                        LOB_MasterID = LOB.LOB_MasterID,
                        TypeOfShop = LOB.TypeOfShop,
                        Description = LOB.Description,
                        LineOfBusiness = LOB.LineOfBusiness,
                        LOBCode = LOB.LOBCode
                    }
                    ).ToList();

            return list;


        }
        public int Insert(LOB_Details ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();

            context.LOB_Details.Add(ObjModel);
            context.SaveChanges();

            return ObjModel.LOB_MasterID;
        }
        public int Update(LOB_Details ObjModel)
        {
            removeCache();
            using var context = new DatabaseContext();
            context.LOB_Details.Update(ObjModel);
            context.SaveChanges();
            return ObjModel.LOB_MasterID;
        }
    }
}
