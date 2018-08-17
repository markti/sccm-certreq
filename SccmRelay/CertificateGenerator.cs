using SccmRelay.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SccmRelay
{
    public class CertificateGenerator : ICertificateGenerator
    {
        private void WriteToLog(string message)
        {
            // write to console
            Console.WriteLine(message);
            // write to event log
            WriteToEventLog(message);
        }
        private void WriteToEventLog(string message)
        {
            string cs = "CertificateGenerator";
            EventLog elog = new EventLog();
            if (!EventLog.SourceExists(cs))
            {
                EventLog.CreateEventSource(cs, "Application");
            }                
            elog.Source = cs;
            elog.EnableRaisingEvents = true;
            elog.WriteEntry(message);
        }

        public byte[] GetCertificate(string hostname)
        {
            byte[] allBytes = null;

            var templateName = ConfigurationManager.AppSettings["TemplateName"];
            var caServerName = ConfigurationManager.AppSettings["CertificateAuthority"];

            // C:\CERTREQ
            var rootPath = ConfigurationManager.AppSettings["CertificatePath"];
            var rootDirectory = new DirectoryInfo(rootPath);
            // C:\CERTREQ\Request.Certificate.ps1
            var powershellFilePath = rootDirectory.FullName + "\\Request-Certificate.ps1";
            var powershellFile = new FileInfo(powershellFilePath);

            try
            {
                // C:\CERTREQ\{GUID}
                var newDirectory = rootDirectory.CreateSubdirectory(Guid.NewGuid().ToString());

                WriteToLog("Creating new sub-directory: " + newDirectory.FullName);
                
                var newPowerShellFilename = newDirectory.FullName + "\\Request-Certificate.ps1";

                // C:\CERTREQ\{GUID}\Request.Certificate.ps1
                var newPowershellFile = powershellFile.CopyTo(newPowerShellFilename);

                WriteToLog("Copied PowerShell File: " + newPowershellFile.FullName);
                
                StringBuilder commandBuilder = new StringBuilder();
                commandBuilder.Append("-ExecutionPolicy bypass");
                commandBuilder.Append(" ");
                commandBuilder.Append("-File ");
                commandBuilder.Append("\"");
                commandBuilder.Append(newPowershellFile.FullName);
                commandBuilder.Append("\"");
                commandBuilder.Append(" ");
                commandBuilder.Append("-CN ");
                commandBuilder.Append(hostname);
                commandBuilder.Append(" ");
                commandBuilder.Append("-TemplateName ");
                commandBuilder.Append(templateName);
                commandBuilder.Append(" ");
                commandBuilder.Append("-CAName ");
                commandBuilder.Append(caServerName);
                commandBuilder.Append(" ");
                commandBuilder.Append("-Export");
                
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "powershell.exe";
                startInfo.Arguments = commandBuilder.ToString();
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.WorkingDirectory = newDirectory.FullName;

                WriteToLog("PowerShell Arguments:" + startInfo.Arguments);
                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();

                WriteToLog(output);

                string errors = process.StandardError.ReadToEnd();

                WriteToLog(errors);

                // C:\CERTREQ\{GUID}\{hostname}.pfx
                var certificatePath = newDirectory.FullName + "\\" + hostname + ".pfx";
                //var certificatePath = "./" + hostname + ".pfx";

                WriteToLog("Converting Certificate File to Bytes:" + certificatePath);

                WriteToLog("About to read certificate here: " + certificatePath);
                allBytes = File.ReadAllBytes(certificatePath); 

            }
            catch (Exception ex)
            {
                WriteToLog(ex.ToString());
            }
            return allBytes;
        }
    }
}