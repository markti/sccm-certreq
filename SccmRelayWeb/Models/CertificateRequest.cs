using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SccmRelayWeb.Models
{
    public class CertificateRequest
    {
        public string HostName { get; set; }
        public bool GenerateRandom { get; set; }
        public string ClientSecret { get; set; }
    }
}