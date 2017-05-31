using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace TVH.ApiApps.ServiceBusClaimCheck.Models
{
    public class QueueMessageCount
    {
        [Metadata("Queue Name")]
        public string queueName { get; set; }

        [Metadata("Message Count")]
        public long messageCount { get; set; }
    }
}