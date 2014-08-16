using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Process = System.Diagnostics.Process;

namespace JsReportVSTools.Impl
{
    /// <summary>
    /// Responsible for managing lifecycle of jsreport nodejs server.
    /// It always keeps just one server running.
    /// </summary>
    public class EmbeddedServerManager : ReportingServerManagerBase, IReportingServerManager
    {
        private dynamic _embeddedReportingServer;
        private string _currentShadowBinFolder;
        private bool _isRunning;

        public string ServerUri
        {
            get { return _embeddedReportingServer.EmbeddedServerUri; }
        }

        public EmbeddedServerManager(string currentShadowBinFolder, object configuration) : base(configuration)
        {
            _currentShadowBinFolder = currentShadowBinFolder;
        }

        /// <summary>
        /// Ensures that jsreport server is running for current project
        /// </summary>
        public RemoteTask<int> EnsureStartedAsync()
        {
            if (_isRunning)
                return RemoteTask.ServerStart((cts) => Task.FromResult(1));

            _isRunning = true;
            return RemoteTask.ServerStart<int>(async cts =>
            {
                await StartAsync().ConfigureAwait(false);
                return 0;
            });
        }

        public RemoteTask<int> StopAsync()
        {
            _isRunning = false;
            return RemoteTask.ServerStart<int>(async cts =>
            {
                await _embeddedReportingServer.StopAsync().ConfigureAwait(false);
                return 0;
            });
        }

        /// <summary>
        /// Creates dynamicly reporting service from bin folder
        /// </summary>
        public override object CreateReportingService()
        {
            Type reportingServiceType = AppDomain.CurrentDomain.Load("jsreport.Client").GetType("jsreport.Client.ReportingService");

            return Activator.CreateInstance(reportingServiceType, ServerUri);
        }
      
        private async Task StartAsync()
        {
            /* We need to copy jsreport embedded dlls together with zipped server to temporary location
             * because windows will lock these files and we don't want to block visual studio from build.
             * We keep separate temporary directory for every project.
            */
       
            if (!File.Exists( Path.Combine(_currentShadowBinFolder, "jsreport.Embedded.dll")))
            {
                throw new WeakJsReportException("Didn't find jsreport.Embedded.dll. Install jsreport.Embedded nuget package or install jsreport.Client and use ReportingStartup.cs to configure remote jsreport.");
            }

            Type embeddedReportingServerType = AppDomain.CurrentDomain.Load("jsreport.Embedded").GetType("jsreport.Embedded.EmbeddedReportingServer");

            _embeddedReportingServer = Activator.CreateInstance(embeddedReportingServerType, FindFreePort());

            _embeddedReportingServer.BinPath = _currentShadowBinFolder;
            await ((Task)_embeddedReportingServer.StartAsync()).ConfigureAwait(false);
        }

        private static long FindFreePort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}