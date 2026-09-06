using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.MenuMaster
{
    public class SupportMenuViewModel
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private int _MenuMasterID;

        public int MenuMasterID
        {
            get => _MenuMasterID;
            set => _MenuMasterID = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        public List<SupportAction> Actions = new List<SupportAction>();
        internal int ParentMenuID;
    }

    public class SupportAction
    {
        private int _MenuMasterID;

        public int MenuMasterID
        {
            get => _MenuMasterID;
            set => _MenuMasterID = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }
    }
}