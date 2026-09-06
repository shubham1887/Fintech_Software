using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.User
{
    public class UserViewWrapper
    {
        public List<UserViewModel> Upline = new List<UserViewModel>();

        private string _AliasName;

        public string AliasName
        {
            get => _AliasName.TrimIfNotNull();
            set => _AliasName = value.TrimIfNotNull();
        }
    }
}