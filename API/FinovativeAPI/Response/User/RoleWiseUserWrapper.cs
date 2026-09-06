using System.Collections.Generic;

namespace Project.Response.User
{
    public class RoleWiseUserWrapper
    {
        private int _Count;

        public List<RoleWiseUserViewModel> list = new List<RoleWiseUserViewModel>();

        public int Count { get => _Count; set => _Count = value; }
    }
}