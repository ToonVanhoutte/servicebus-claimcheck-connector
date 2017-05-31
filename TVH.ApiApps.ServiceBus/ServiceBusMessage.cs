using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRex.Metadata;

namespace TVH.ApiApps.ServiceBus
{
    public class ServiceBusMessage
    {        
            public byte[] Content { get; set; }
            public string ContentType { get; set; }
            public IDictionary<string,object> Properties { get; set; }
            public DateTime ScheduledEnqueueTimeUtc { get; set; }
            public string LockToken { get; set; }
    }
}
