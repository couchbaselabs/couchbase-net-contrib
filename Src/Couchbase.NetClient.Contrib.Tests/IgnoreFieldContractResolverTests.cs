using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.NetClient.Contrib.ContractResolvers;
using Couchbase.NetClient.Contrib.Tests.POCOs;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Couchbase.NetClient.Contrib.Tests
{
    [TestFixture]
    public class IgnoreFieldContractResolverTests : IgnoreFieldContractResolver
    {
        [Test]
        public void When_POCO_Contains_FieldToIgnore_CreateProperties_Will_Not_Return_It()
        {
            var contractResolver = new IgnoreFieldContractResolverTests
            {
                FieldToIgnore = "_id"
            };
            var properties = contractResolver.CreateProperties(typeof (QueueItem), new MemberSerialization());
            var property = properties.FirstOrDefault(x => x.PropertyName == "_id");
            Assert.IsNull(property);
        }
    }
}
