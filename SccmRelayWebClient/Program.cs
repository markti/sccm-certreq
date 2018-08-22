using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Configuration;

namespace SccmRelayWebClient
{

    /// <summary>
    /// this client is for machines that do not have powershell 3.0
    /// powershell 3.0 introduced the Invoke-Webrequest command
    /// powershell 3.0 requires full version of .net framework 4.0
    /// </summary>
    class Program
    {
        private static string ClientSecret = "";
        private static string RemoteHost = "";

        static void Main(string[] args)
        {
            ClientSecret = ConfigurationManager.AppSettings["ApplicationKey"];
            RemoteHost = ConfigurationManager.AppSettings["RemoteHost"];

            bool isSuccess = false;
            int retryCount = 0;
            int maxRetryCount = 3;
            int coolDownPeriodMinutes = 10;

            int.TryParse(ConfigurationManager.AppSettings["RetryCount"], out maxRetryCount);
            int.TryParse(ConfigurationManager.AppSettings["CooldownPeriod"], out coolDownPeriodMinutes);

            TimeSpan cooldownPeriod = TimeSpan.FromMinutes(coolDownPeriodMinutes);

            var machineName = Environment.MachineName;

            // while it is not successful for the actual retries is less than our max, keep trying
            while (!isSuccess && retryCount < maxRetryCount)
            {
                isSuccess = GetCertificate();
                retryCount++;
                if(!isSuccess)
                {
                    LogEcxeption("retry attempt " + retryCount + " failed. sleeping for " + cooldownPeriod.TotalSeconds + " seconds");
                    Thread.Sleep(cooldownPeriod);
                }
            }
            if(!isSuccess)
            {
                LogEcxeption("Retried " + maxRetryCount + " failed everytime");
            }
        }
        
        public static bool GetCertificate()
        {
            bool isSuccess = false;
            try
            {
                LogCertificateRequested();

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;

                var postUri = new Uri(RemoteHost + "/api/Certificate");

                var request = (HttpWebRequest)WebRequest.Create(postUri);

                int timeoutMin = 2;
                int.TryParse(ConfigurationManager.AppSettings["Timeout"], out timeoutMin);
                request.Timeout = 1000 * 60 * timeoutMin;

                var machineName = Environment.MachineName;
                var postData = "{ 'hostName': '" + machineName + "', 'clientSecret': '" + ClientSecret + "' }";
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseStream = response.GetResponseStream();
                StreamReader sr = new StreamReader(responseStream);

                var certOutFilename = ConfigurationManager.AppSettings["OutputFilename"];

                if (File.Exists(certOutFilename))
                {
                    File.Delete(certOutFilename);
                }
                var fileStream = File.OpenWrite(certOutFilename);

                CopyStream(responseStream, fileStream);

                fileStream.Flush();
                fileStream.Close();

                LogCertificateReceived();

                isSuccess = true;
            }
            catch (Exception ex)
            {
                LogEcxeption(ex.ToString());
            }
            return isSuccess;
        }
        public static void LogCertificateRequested()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;

            var postUri = new Uri(RemoteHost.Replace("https","http") + "/api/event/req");

            var request = (HttpWebRequest)WebRequest.Create(postUri);

            var machineName = Environment.MachineName;
            var postData = "{ 'hostName': '" + machineName + "' }";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
        }

        public static void LogCertificateReceived()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;

            var postUri = new Uri(RemoteHost.Replace("https", "http") + "/api/event/ack");

            var request = (HttpWebRequest)WebRequest.Create(postUri);

            var machineName = Environment.MachineName;
            var postData = "{ 'hostName': '" + machineName + "' }";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
        }

        public static void LogEcxeption(string errorMessage)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;

            var postUri = new Uri(RemoteHost.Replace("https", "http") + "/api/event/error");

            var request = (HttpWebRequest)WebRequest.Create(postUri);

            var machineName = Environment.MachineName;
            var jsonReq = new LogErrorRequest();
            jsonReq.HostName = machineName;
            jsonReq.ErrorMessage = errorMessage;

            var jsonData = JsonConvert.SerializeObject(jsonReq);

            //var postData = "{ 'hostName': '" + machineName + "', 'errorMessage': '" + errorMessage + "', 'clientSecret': '" + ClientSecret + "' }";
            var data = Encoding.ASCII.GetBytes(jsonData);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
        }

        public class LogErrorRequest
        {
            public string HostName { get; set; }
            public string ErrorMessage { get; set; }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }
    }
}