using Microsoft.ApplicationInsights;
using SccmRelayWeb.Models;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SccmRelayWeb.Controllers
{
    [Route("api/Event")]
    public class EventController : ApiController
    {
        private TelemetryClient telemetry = new TelemetryClient();

        [SwaggerOperation("LogClientError")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        [Route("api/event/error")]
        public HttpResponseMessage LogError([FromBody]LogErrorRequest request)
        {
            HttpResponseMessage result = null;


            if (request == null)
            {
                result = Request.CreateResponse(HttpStatusCode.InternalServerError);
                return result;
            }

            var env = ConfigurationManager.AppSettings["EnvironmentName"];

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Hostname", request.HostName);
            parameters.Add("ErrorMessage", request.ErrorMessage);
            parameters.Add("Environment", env);
            parameters.Add("EventName", "error");

            telemetry.TrackEvent("ClientError", parameters);

            result = Request.CreateResponse(HttpStatusCode.OK);

            return result;
        }


        [SwaggerOperation("LogClientRequest")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        [Route("api/event/req")]
        public HttpResponseMessage LogCertReq([FromBody]LogCertificateReqRequest request)
        {
            HttpResponseMessage result = null;
            
            if (request == null)
            {
                result = Request.CreateResponse(HttpStatusCode.InternalServerError);
                return result;
            }

            var env = ConfigurationManager.AppSettings["EnvironmentName"];

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Hostname", request.HostName);
            parameters.Add("Environment", env);
            parameters.Add("EventName", "req");

            telemetry.TrackEvent("ClientRequest", parameters);

            result = Request.CreateResponse(HttpStatusCode.OK);

            return result;
        }

        
        [SwaggerOperation("LogClientAck")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        [Route("api/event/ack")]
        public HttpResponseMessage LogCertAck([FromBody]LogCertificateAckRequest request)
        {
            HttpResponseMessage result = null;


            if (request == null)
            {
                result = Request.CreateResponse(HttpStatusCode.InternalServerError);
                return result;
            }

            var env = ConfigurationManager.AppSettings["EnvironmentName"];

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("Hostname", request.HostName);
            parameters.Add("Environment", env);
            parameters.Add("EventName", "ack");

            telemetry.TrackEvent("ClientAck", parameters);

            result = Request.CreateResponse(HttpStatusCode.OK);

            return result;
        }
    }
}