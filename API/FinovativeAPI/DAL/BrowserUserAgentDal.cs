using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class BrowserUserAgentDal
    {
        public static readonly string Key = "_UserAgent";

        internal BrowserUserAgentMaster SelectByUserAgent(string UserAgent)
        {
            List<BrowserUserAgentMaster> list = Select();
            return list.FirstOrDefault(l => l.UserAgent == UserAgent);
        }

        internal List<BrowserUserAgentMaster> Select()
        {
            var _list = new MemoryCacheProcessing<List<BrowserUserAgentMaster>>();
            List<BrowserUserAgentMaster> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }

        private List<BrowserUserAgentMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            return context.BrowserUserAgentMaster.ToList();
        }
    }
}