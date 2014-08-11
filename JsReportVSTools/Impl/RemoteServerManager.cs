using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace JsReportVSTools.Impl
{
    public class RemoteServerManager : IReportingServerManager
    {
        private readonly string _binFolder;
        private readonly dynamic _configuration;

        public RemoteServerManager(string binFolder, object configuration)
        {
            _binFolder = binFolder;
            _configuration = configuration;
        }

        public Task StopAsync()
        {
            return Task.FromResult(0);
        }

        public Task EnsureStartedAsync(string fileName = null)
        {
            return Task.FromResult(0);
        }

        public object CreateReportingService()
        {
            Type reportingServiceType = Assembly.Load(Path.Combine(_binFolder, "jsreport.Client.dll"))
                    .GetType("jsreport.Client.ReportingService");

            return Activator.CreateInstance(reportingServiceType, ServerUri, _configuration.RemoteServerUsername, _configuration.RemoteServerUsername);
        }

        public string ServerUri { get { return _configuration.RemoteServerUri; }}

        
    }
}