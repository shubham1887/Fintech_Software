using Project.Model;
using Project.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class RefreshTokenDal
    {
        public static string Key
        { get { return "_RefreshTokenMaster"; } }

        private void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<RefreshTokenMaster>>();
            cache.Remove(Key);
        }

        internal void Remove(RefreshTokenMaster tokenObj)
        {
            using var context = new DatabaseContext();
            context.RefreshTokenMaster.Remove(tokenObj);
            context.SaveChanges();
            RemoveCache();
        }

        internal void RemoveAll()
        {
            List<RefreshTokenMaster> allTokens = SelectWithoutCache();
            using var context = new DatabaseContext();
            context.RefreshTokenMaster.RemoveRange(allTokens);
            context.SaveChanges();
            RemoveCache();
        }

        internal void Insert(RefreshTokenMaster tokenObj)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.RefreshTokenMaster.Add(tokenObj);
            context.SaveChanges();
        }

        private List<RefreshTokenMaster> SelectAll()
        {
            var _refreshTokenMasterList = new MemoryCacheProcessing<List<RefreshTokenMaster>>();
            List<RefreshTokenMaster> refreshTokenMasterList = _refreshTokenMasterList.GetOrCreate(Key, () => SelectWithoutCache());

            return refreshTokenMasterList.OrderByDescending(c => c.DOC).ToList();
        }

        private List<RefreshTokenMaster> SelectWithoutCache()
        {
            List<RefreshTokenMaster> list = new List<RefreshTokenMaster>();
            using (var context = new DatabaseContext())
            {
                list = context.RefreshTokenMaster.ToList();
            }
            return list;
        }

        internal RefreshTokenMaster SelectByID(int userID)
        {
            List<RefreshTokenMaster> tokens = SelectAll();
            return tokens.FirstOrDefault(t => t.UserID == userID);
        }

        internal void Update(RefreshTokenMaster refreshToken)
        {
            RemoveCache();
            using var context = new DatabaseContext();
            context.RefreshTokenMaster.Update(refreshToken);
            context.SaveChanges();
        }

        internal RefreshTokenMaster SelectByToken(string refreshToken, int UserID)
        {
            RefreshTokenMaster objmodel = null;
            List<RefreshTokenMaster> tokens = SelectAll();
            try
            {
                objmodel = tokens.FirstOrDefault(t => t.RefreshToken == refreshToken && t.UserID == UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return objmodel;
        }
    }
}