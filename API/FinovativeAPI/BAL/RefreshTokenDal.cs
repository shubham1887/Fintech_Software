using Project.DAL;
using Project.Model;

namespace Project.BAL
{
    public class RefreshTokenBal
    {
        internal void Remove(int UserID)
        {
            RefreshTokenMaster objModel = new RefreshTokenDal().SelectByID(UserID);

            try
            {
                new RefreshTokenDal().Remove(objModel);
            }
            catch (System.Exception ex)
            {
            }
        }

        internal void RemoveAll()
        {
            try
            {
                new RefreshTokenDal().RemoveAll();
            }
            catch (System.Exception ex)
            {
            }
        }
    }
}