using System;
using System.Threading.Tasks;

namespace JsReportVSTools.Impl
{
    public class RemoteServerManager : ReportingServerManagerBase, IReportingServerManager
    {
        private readonly string _remoteServerUri;
        private readonly string _username;
        private readonly string _password;

        public RemoteServerManager(string remoteServerUri, string username, string password, object configuration) : base(configuration)
        {
            _remoteServerUri = remoteServerUri;
            _username = username;
            _password = password;
        }

        public RemoteTask<int> StopAsync()
        {
            return RemoteTask.ServerStart(cts => Task.FromResult(0));
        }

        public RemoteTask<int> EnsureStartedAsync()
        {
            return RemoteTask.ServerStart(cts => Task.FromResult(0));
        }

        public override object CreateReportingService()
        {
            Type reportingServiceType = AppDomain.CurrentDomain.Load("jsreport.Client").GetType("jsreport.Client.ReportingService");

            return Activator.CreateInstance(reportingServiceType, _remoteServerUri, _username, _password);
        }

        public string ServerUri { get { return _remoteServerUri; } }
        
    }

    
}