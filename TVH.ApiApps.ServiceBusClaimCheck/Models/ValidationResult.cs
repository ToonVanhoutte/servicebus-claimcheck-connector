using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace TVH.ApiApps.ServiceBusClaimCheck.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }  

        public HttpResponseMessage HttpResult { get; set; }
    }
}