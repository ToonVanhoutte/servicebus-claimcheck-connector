using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TVH.RoutingSlip
{
    public static class RoutingSlipClient
    {

        public static string SetNextRouting(string routingSlipXml, Dictionary<string,string> contextProperties, out RoutingStep nextRoutingStep)
        {
            var routingSlip = DeserializeRoutingSlipXml(routingSlipXml);

            if(contextProperties != null)
            {
                var newContextProperties = contextProperties.Select(p => new Parameter { Name = p.Key, Value = p.Value }).ToList();
                routingSlip.RoutingHeader.Context.AddRange(newContextProperties);
            }
                        
            if (routingSlip.RoutingHeader.CurrentStep < routingSlip.RoutingSteps.Count)
            {
                routingSlip.RoutingHeader.CurrentStep += 1;

                nextRoutingStep = routingSlip.RoutingSteps[routingSlip.RoutingHeader.CurrentStep - 1];
                return SerializeRoutingSlipObject(routingSlip);
            }

            nextRoutingStep = null;
            return null;
        }

        public static RoutingStep GetCurrentRoutingStep(string routingSlipXml)
        {
            var routingSlip = DeserializeRoutingSlipXml(routingSlipXml);
            var currentRoutingStep = routingSlip.RoutingSteps[routingSlip.RoutingHeader.CurrentStep - 1];
            return currentRoutingStep;
        }

        public static Dictionary<string,string> GetCurrentStepProperties(string routingSlipXml)
        {
            var routingSlip = DeserializeRoutingSlipXml(routingSlipXml);
            var currentRoutingStep = routingSlip.RoutingSteps[routingSlip.RoutingHeader.CurrentStep - 1];
            var currentStepProperties = currentRoutingStep.StepConfig.ToDictionary(x => x.Name, x => x.Value);
            var contextProperties = GetContext(routingSlip);

            contextProperties.Add("Guid", Guid.NewGuid().ToString());
            var regex = new Regex(@"\$\(([A-Za-z0-9\-]+)\)");

            return currentStepProperties.ToDictionary(x => x.Key, x => regex.Replace(x.Value, c => contextProperties[c.Groups[1].Value]));
        }

        public static string InjectRoutingSlip(string routingSlipXml, string routingSlipToInject)
        {
            var routingSlip = DeserializeRoutingSlipXml(routingSlipXml);
            var routingSlipToAppend = DeserializeRoutingSlipXml(routingSlipToInject);

            routingSlip.RoutingSteps.RemoveAt(routingSlip.RoutingHeader.CurrentStep - 1);
            routingSlip.RoutingSteps.InsertRange(routingSlip.RoutingHeader.CurrentStep - 1, routingSlipToAppend.RoutingSteps);

            return SerializeRoutingSlipObject(routingSlip);
        }

        public static Dictionary<string, string> GetContext(string routingSlipXml)
        {
            var routingSlip = DeserializeRoutingSlipXml(routingSlipXml);
            return GetContext(routingSlip);
        }

        public static Dictionary<string, string> GetContext(RoutingSlip routingSlip)
        {
            return routingSlip.RoutingHeader.Context.ToDictionary(x => x.Name, x => x.Value);
        }

        public static RoutingHeader GetRoutingSlipHeader(string routingSlipXml)
        {
            var routingSlip = DeserializeRoutingSlipXml(routingSlipXml);
            return routingSlip.RoutingHeader;
        }

        private static RoutingSlip DeserializeRoutingSlipXml(string routingSlipXml)
        {
            var serializer = new XmlSerializer(typeof(RoutingSlip));
            return (RoutingSlip)serializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(routingSlipXml)));
        }

        private static string SerializeRoutingSlipObject(RoutingSlip routingSlip)
        {
            var serializer = new XmlSerializer(typeof(RoutingSlip));
            var outputStream = new MemoryStream();
            serializer.Serialize(outputStream, routingSlip);
            outputStream.Seek(0, SeekOrigin.Begin);
            return new StreamReader(outputStream).ReadToEnd();
        }
    }
}
