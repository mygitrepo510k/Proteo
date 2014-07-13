using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Attributes;

namespace MWF.Mobile.Tests.RepositoryTests.TestModels
{
    public class ParentEntity : IBlueSphereEntity
    {

        [Unique]
        public Guid ID { get; set; }

        public string Title { get; set; }

        [ForeignKey(typeof(GrandParentEntity))]
        public Guid ParentID { get; set; }

        [ChildRelationship(typeof(ChildEntity))]
        public List<ChildEntity> Children { get; set; }

        [ChildRelationship(typeof(ChildEntity2))]
        public List<ChildEntity2> Children2 { get; set; }
    
    }
}
