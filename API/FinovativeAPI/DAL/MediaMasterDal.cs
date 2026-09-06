using Project.Model;

namespace Project.DAL
{
    public class MediaMasterDal
    {
        internal int Insert(MediaMaster objModel)
        {
            using (var context = new DatabaseContext())
            {
                context.MediaMaster.Add(objModel);
                context.SaveChanges();
                return objModel.MediaMasterID;
            }
        }
    }
}