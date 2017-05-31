using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVH.ServiceBus
{
    public class ServiceBusMessage
    {
        public ServiceBusMessage()
        {
            Properties = new Dictionary<string, string>();
        }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public DateTime ScheduledEnqueueTimeUtc { get; set; }
        public string LockToken { get; set; }
    }
}
