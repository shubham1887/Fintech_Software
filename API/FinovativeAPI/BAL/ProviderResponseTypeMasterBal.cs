using Project.DAL;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ProviderResponseTypeMasterBal
    {
        internal List<Response.ProviderResponseType.ViewModel> Select()
        {
            return new ProviderResponseTypeMasterDal().Select();
        }

        internal List<string> SelectINString()
        {
            List<string> types = new List<string>();
            List<Response.ProviderResponseType.ViewModel> typelist = new List<Response.ProviderResponseType.ViewModel>();
            typelist = Select();
            foreach (Response.ProviderResponseType.ViewModel type in typelist)
            {
                types.Add(type.Type);
            }

            return types;
        }
    }
}