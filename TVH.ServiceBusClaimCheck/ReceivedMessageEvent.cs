using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRex.Metadata;

namespace TVH.ServiceBusClaimCheck
{
    public class ReceivedMessageEvent
    {
        [Metadata("Content", "Content of the message", VisibilityType.Default)]
        public byte[] Content { get; set; }

        [Metadata("Content Type", "Content type of the message content", VisibilityType.Default)]
        public string ContentType { get; set; }

        [Metadata("Lock Token", "Identifier to abandon or complete the message", VisibilityType.Default)]
        public string LockToken { get; set; }

        [Metadata("Properties", "Message properties", VisibilityType.Default)]
        public IDictionary<string, object> Properties { get; set; }
    }
}
