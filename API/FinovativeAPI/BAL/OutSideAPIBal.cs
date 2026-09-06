using Project.DAL;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class OutSideAPIBal
    {
        internal string SelectHelplineNumber()
        {
            string HelplineNumber = new SettingMasterDal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.HelplineNumber));

            return HelplineNumber;
        }

        internal string SelectPlayStoreURL()
        {
            string PlayStoreURL = new SettingMasterDal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.PlaystoreURL));

            return PlayStoreURL;
        }
    }
}