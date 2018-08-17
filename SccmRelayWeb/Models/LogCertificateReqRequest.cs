using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SccmRelayWeb.Models
{
    public class LogCertificateReqRequest
    {
        public string HostName { get; set; }
        public string ClientSecret { get; set; }
    }
}