using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;

namespace MWF.Mobile.Core.Models.Attributes
{

    [AttributeUsage(AttributeTargets.Property)]
    public class ChildRelationshipAttribute : IgnoreAttribute
    {
        public ChildRelationshipAttribute(Type childType)
        {
            // childType must implement IBlueSphereEntity
            //Contract.Requires<ArgumentException>(typeof(IBlueSphereEntity).GetTypeInfo().IsAssignableFrom(childType.GetTypeInfo()), 
            //                                    "ChildRelationshipAttribute should only apply to types that implement IBluesphereEntity");

            ChildType = childType;
        }

        public Type ChildType { get; private set; }
    }

}
