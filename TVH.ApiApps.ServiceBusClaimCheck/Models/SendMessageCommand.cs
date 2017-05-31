using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;
using TVH.ApiApps.ServiceBusClaimCheck.Controllers;

namespace TVH.ApiApps.ServiceBusClaimCheck.Models
{
    public class SendMessageCommand
    {
        [Metadata("Content", "Content of the message", VisibilityType.Default)]
        public byte[] Content { get; set; }

        [Metadata("Content Type", "Content type of the message content", VisibilityType.Default)]
        public string ContentType { get; set; }

        [Metadata("Queue Name", "Name of the queue", VisibilityType.Default)]
        public string QueueName { get; set; }

        [Metadata("Properties", "Message properties in JSON format", VisibilityType.Default)]
        public JObject Properties { get; set; }

        [Metadata("Scheduled Enqueue Time in UTC", "UTC time in MM/dd/yyyy HH:mm:ss format", VisibilityType.Advanced)]
        public string ScheduledEnqueueTimeUtc { get; set; }
    }
}