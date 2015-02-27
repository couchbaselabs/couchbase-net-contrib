﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using NUnit.Framework;
using Couchbase.NetClient.Contrib;
using Couchbase.NetClient.Contrib.ContractResolvers;
using Couchbase.NetClient.Contrib.Tests.POCOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Couchbase.NetClient.Contrib.Tests
{
    [TestFixture]
    public class BucketExtensionTests
    {
        readonly ICluster _cluster = new Cluster();
        private IBucket _bucket;

        [SetUp]
        public void SetUp()
        {
            _bucket = _cluster.OpenBucket();
        }

        [Test]
        public void Test_InsertFromJson()
        {
            _bucket.Remove("25892e17-80f6-415f-9c65-7395632f0223");
            var json = File.ReadAllText("Data\\doc-with-embedded-id.json");
            var result = _bucket.ExtractKeyAndInsert<dynamic>(json, "_id");
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void When_Id_Field_Exists_It_Is_Removed_From_Document()
        {
            _bucket.Remove("25892e17-80f6-415f-9c65-7395632f0223");
            var json = File.ReadAllText("Data\\doc-with-embedded-id.json");

            string id;
            var insert = _bucket.ExtractKeyAndInsert<dynamic>(json, "_id", out id);
            Assert.IsTrue(insert.Success);

            var get = _bucket.GetDocument<dynamic>(id);
            Assert.IsTrue(get.Success);
            Assert.IsNull(get.Content._id);
        }

        [Test]
        public void When_Id_Field_Exists_It_Is_Removed_From_Document_Object()
        {
            _bucket.Remove("25892e17-80f6-415f-9c65-7395632f0223");
            var json = File.ReadAllText("Data\\doc-with-embedded-id.json");

            string id;
            var insert = _bucket.ExtractKeyAndInsert<object>(json, "_id", out id);
            Assert.IsTrue(insert.Success);

            var get = _bucket.GetDocument<dynamic>(id);
            Assert.IsTrue(get.Success);
            Assert.IsNull(get.Content._id);
        }


        [Test]
        [ExpectedException(typeof(JsonReaderException))]
        public void When_T_Is_String_JsonReaderException_Is_Thrown()
        {
            _bucket.Remove("25892e17-80f6-415f-9c65-7395632f0223");
            var json = File.ReadAllText("Data\\doc-with-embedded-id.json");

            string id;
            var insert = _bucket.ExtractKeyAndInsert<string>(json, "_id", out id);//should be fixed at some point
            Assert.IsTrue(insert.Success);

            var get = _bucket.GetDocument<dynamic>(id);
            Assert.IsTrue(get.Success);
            Assert.IsNull(get.Content._id);
        }

        [Test]
        public void When_Type_Is_Poco_Id_Is_Removed_From_Document()
        {

            using (var cluster = new Cluster(new ClientConfiguration
            {
                SerializationSettings =
                    new JsonSerializerSettings
                    {
                        ContractResolver = new IgnoreFieldContractResolver("_id")
                    }
            }))
            {
                using (var bucket = cluster.OpenBucket())
                {
                    bucket.Remove("25892e17-80f6-415f-9c65-7395632f0223");
                    var json = File.ReadAllText("Data\\doc-with-embedded-id.json");

                    var id = string.Empty;
                    bucket.ExtractKeyAndInsert<QueueItem>(json, "Id", out id);
                    var get = bucket.GetDocument<dynamic>(id);

                    Assert.IsNull(get.Content._id);
                }
            }
        }

        public void TearDown()
        {
            _cluster.CloseBucket(_bucket);
            _cluster.Dispose();
        }
    }
}
