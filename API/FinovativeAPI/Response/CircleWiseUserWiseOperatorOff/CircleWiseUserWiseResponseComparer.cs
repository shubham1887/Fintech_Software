using System;
using System.Collections.Generic;

namespace Project.Response.CircleWiseUserWiseOperatorOff
{
    public class CircleWiseUserWiseResponseComparer : IEqualityComparer<ViewModel>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(ViewModel x, ViewModel y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.UserID == y.UserID && x.OperatorID == y.OperatorID;
        }

        public int GetHashCode(ViewModel x)
        {
            return this.GetHashCode();
        }
    }
}