using Microsoft.ApplicationInsights;
using Microsoft.ServiceBus;
using SccmRelay.Interface;
using SccmRelayWeb.Models;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Web.Http;

namespace SccmRelayWeb.Controllers
{
    public class CertificateController : ApiController
    {
        private TelemetryClient telemetry = new TelemetryClient();

        // GET api/values/5
        [SwaggerOperation("GetByHostname")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        public HttpResponseMessage Post([FromBody]CertificateRequest request)
        {
            HttpResponseMessage result = null;
            

            if (request == null)
            {
                result = Request.CreateResponse(HttpStatusCode.InternalServerError);
                return result;
            }

            var actualSecretKey = ConfigurationManager.AppSettings["ClientSecretKey"];

            if(string.IsNullOrEmpty(request.ClientSecret) || !request.ClientSecret.Equals(actualSecretKey))
            {
                result = Request.CreateResponse(HttpStatusCode.InternalServerError);
                return result;
            }

            var hostName = "client-endpoint";

            if (!string.IsNullOrEmpty(request.HostName))
            {
                hostName = request.HostName;
            }

            if(request.GenerateRandom)
            {
                hostName = "client-" + Guid.NewGuid().ToString().Replace("-", "");
            }

            byte[] data = null;

            var serviceNamespace = ConfigurationManager.AppSettings["ServiceNamespace"];
            var serviceKey = ConfigurationManager.AppSettings["ServiceKey"];
            var serviceKeyName = ConfigurationManager.AppSettings["ServiceKeyName"];
            var servicePath = ConfigurationManager.AppSettings["ServicePath"];

            Console.WriteLine("HOSTNAME: " + hostName);

            var cf = new ChannelFactory<ICertificateGeneratorChannel>(
               new NetTcpRelayBinding(),
                new EndpointAddress(ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, servicePath)));

            cf.Endpoint.Behaviors.Add(new TransportClientEndpointBehavior
            { TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(serviceKeyName, serviceKey) });

            using (var ch = cf.CreateChannel())
            {
                data = ch.GetCertificate(hostName);
            }
            result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(new MemoryStream(data));
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "client.pfx";
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return result;
        }
    }
}