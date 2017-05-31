using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVH.RoutingSlip
{
    public class RoutingHeader
    {
        public RoutingHeader()
        {
            CorrelationId = Guid.NewGuid().ToString();
            Context = new List<Parameter>();
        }
        public string CorrelationId { get; set; }
        public string Flow { get; set; }
        public int CurrentStep { get; set; }
        public List<Parameter> Context { get; set; }
    }
}
