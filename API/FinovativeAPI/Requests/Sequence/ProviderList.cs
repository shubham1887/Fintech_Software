using Project.Processing;
using System;
using System.Collections.Generic;

namespace Project.Requests.Sequence
{
    public class ProviderList
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        public string IsActivateString
        {
            get => IsActive ? " activated" : " deactivated";
        }

        public override string ToString()
        {
            return $"Name:{ProviderName}, {IsActivateString}";
        }
    }

    internal class ProviderListComparer : IEqualityComparer<ProviderList>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(ProviderList x, ProviderList y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.ProviderID == y.ProviderID;
        }

        public int GetHashCode(ProviderList provider)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(provider, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashProductName = provider.ProviderID == null ? 0 : provider.ProviderID.GetHashCode();

            //Get hash code for the Code field.
            int hashProductCode = provider.ProviderName.GetHashCode();

            //Calculate the hash code for the product.
            return hashProductName ^ hashProductCode;
        }
    }
}