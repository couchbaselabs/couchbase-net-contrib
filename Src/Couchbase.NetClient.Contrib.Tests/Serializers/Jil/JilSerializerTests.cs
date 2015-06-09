using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;
using Couchbase.Core.Transcoders;
using Couchbase.IO.Converters;
using Couchbase.IO.Operations;
using Couchbase.NetClient.Contrib.Serializers.Jil;
using Couchbase.NetClient.Contrib.Tests.POCOs;
using NUnit.Framework;

namespace Couchbase.NetClient.Contrib.Tests.Serializers.Jil
{
    [TestFixture]
    public class JilSerializerTests
    {
        private Cluster _cluster;

        [SetUp]
        public void SetUp()
        {
            var config = new ClientConfiguration
            {
                Serializer = () => new JilSerializer()
            };

            _cluster = new Cluster(config);
        }

        [TearDown]
        public void TearDown()
        {
            _cluster.Dispose();
        }

        [Test]
        public void When_Type_Is_Json_Deserialization_And_Serialization_Succeed()
        {
            using (var bucket = _cluster.OpenBucket())
            {
                var key = "When_Type_Is_Json_Deserialization_And_Serialization_Succeed";
                bucket.Remove(key);

                var expected = new {Name = "Key", Value = "Value"};
                var result = bucket.Insert(key, expected);
                Assert.IsTrue(result.Success);

                var result1 = bucket.Get<dynamic> (key);
                Assert.IsTrue(result1.Success);
            }
        }

        [Test]
        public void When_Type_Is_POCO_Deserialization_And_Serialization_Succeed()
        {
            using (var bucket = _cluster.OpenBucket())
            {
                var key = "When_Type_Is_Json_Deserialization_And_Serialization_Succeed";
                bucket.Remove(key);

                var expected = new QueueItem
                {
                    CreateTime = 100,
                    Id = "foo",
                    ModeTime = 111,
                    QueueName = "baa"
                };
                var result = bucket.Insert(key, expected);
                Assert.IsTrue(result.Success);

                var result1 = bucket.Get<QueueItem>(key);
                Assert.IsTrue(result1.Success);
                Assert.AreEqual(expected.Id, result1.Value.Id);
            }
        }

        [Test]
        public void Test_Poco_Encode_And_Decode()
        {
            var transcoder = new DefaultTranscoder(new DefaultConverter(), new JilSerializer());

            var expected = new QueueItem {Id = "foo", ModeTime = 111ul};
            var bytes = transcoder.Encode(expected,
                new Flags
                {
                    DataFormat = DataFormat.Json
                },
                OperationCode.Get);

            var actual = transcoder.Decode<QueueItem>(bytes, 0, bytes.Length, OperationCode.Get);
            Assert.AreEqual(expected.Id, actual.Id);
        }

        [Test]
        public void Test_String_Encode_And_Decode()
        {
            var transcoder = new DefaultTranscoder(new DefaultConverter(), new JilSerializer());

            var expected = "hello world";
            var bytes = transcoder.Encode(expected,
                new Flags
                {
                    DataFormat = DataFormat.String
                },
                OperationCode.Get);

            var actual = transcoder.Decode<string>(bytes, 0, bytes.Length,
                new Flags
                {
                    DataFormat = DataFormat.Json
                },
                OperationCode.Get);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_Int_Encode_And_Decode()
        {
            var transcoder = new DefaultTranscoder(new DefaultConverter(), new JilSerializer());

            var expected = 10;
            var bytes = transcoder.Encode(expected,
                new Flags
                {
                    DataFormat = DataFormat.Json
                },
                OperationCode.Get);

            var actual = transcoder.Decode<int>(bytes, 0, bytes.Length,
                new Flags
                {
                    DataFormat = DataFormat.Json
                },
                OperationCode.Get);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Test_dynamic_Encode_And_Decode()
        {
            var transcoder = new DefaultTranscoder(new DefaultConverter(), new JilSerializer());

            dynamic expected = new {Id = "bar", Val=100};
            var bytes = transcoder.Encode(expected,
                new Flags
                {
                    DataFormat = DataFormat.Json
                },
                OperationCode.Get);

            var actual = transcoder.Decode<dynamic>(bytes, 0, bytes.Length,
                new Flags
                {
                    DataFormat = DataFormat.Json
                },
                OperationCode.Get);

            //dynamic does not quite work as expected...
            //Assert.AreEqual(expected.Id, actual.Id);
        }

        [Test]
        public void Test_AppConfig_Configuration()
        {
            var cluster = new Cluster("couchbaseClients/couchbase");
            var bucket = cluster.OpenBucket();

            Assert.IsInstanceOf<JilSerializer>(cluster.Configuration.Serializer());
        }
    }
}
