using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Models
{
    public interface IBlueSphereEntity
    {
        Guid ID { get; set; }
        string Title { get; set; }
    }

    public interface IBlueSphereParentEntity<T> : IBlueSphereEntity where T : IBlueSphereChildEntity
    {
        List<T> Children { get; set; } 
    }

    public interface IBlueSphereChildEntity : IBlueSphereEntity
    {

    }
}
