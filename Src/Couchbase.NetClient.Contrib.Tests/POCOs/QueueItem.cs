using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Couchbase.NetClient.Contrib.Tests.POCOs
{
    public class QueueItem
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        public string QueueName { get; set; }
        public ulong ModeTime { get; set; }
        public ulong CreateTime { get; set; }
    }
}
