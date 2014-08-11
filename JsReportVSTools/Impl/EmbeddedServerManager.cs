using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Process = System.Diagnostics.Process;

namespace JsReportVSTools.Impl
{
    /// <summary>
    /// Responsible for managing lifecycle of jsreport nodejs server.
    /// It always keeps just one server running.
    /// </summary>
    public class EmbeddedServerManager : MarshalByRefObject, IReportingServerManager
    {
        private readonly DTE2 _dte;
        
        private dynamic _embeddedReportingServer;
        private string _currentShadowBinFolder { get; set; }
        private readonly Project _activeProject;

        public string ServerUri
        {
            get { return _embeddedReportingServer.EmbeddedServerUri; }
        }

        public EmbeddedServerManager(DTE2 dte, Project activeProject, string currentShadowBinFolder)
        {
            _dte = dte;
            _activeProject = activeProject;
            
            _currentShadowBinFolder = currentShadowBinFolder;
        }

        /// <summary>
        /// Ensures that jsreport server is running for current project
        /// </summary>
        public async Task EnsureStartedAsync(string fileName = null)
        {
            if (Process.GetProcessesByName("node").Any(p => GetMainModuleFilePath(p.Id).Contains(_currentShadowBinFolder)))
                return;

            await StartAsync(fileName).ConfigureAwait(false);
        }

        public Task StopAsync()
        {
            return _embeddedReportingServer.StopAsync();
        }

        /// <summary>
        /// Creates dynamicly reporting service from bin folder
        /// </summary>
        public object CreateReportingService()
        {
            Type reportingServiceType =
                Assembly.LoadFrom(Path.Combine(_currentShadowBinFolder, "jsreport.Client.dll"))
                    .GetType("jsreport.Client.ReportingService");

            return Activator.CreateInstance(reportingServiceType, ServerUri);
        }
      
        private async Task StartAsync(string fileName = null)
        {
            /* We need to copy jsreport embedded dlls together with zipped server to temporary location
             * because windows will lock these files and we don't want to block visual studio from build.
             * We keep separate temporary directory for every project.
            */
            
            string pathToDll = Path.Combine(_currentShadowBinFolder, "jsreport.Embedded.dll");

            if (!File.Exists(pathToDll))
            {
                throw new MissingJsReportEmbeddedDllException("jsreport.Embedded.dll was not found in path " + pathToDll +
                                                              " . Aren't you missing nuget package jsreport.Embedded?");
                
            }

            Type embeddedReportingServerType =
                Assembly.LoadFrom(Path.Combine(_currentShadowBinFolder, "jsreport.Embedded.dll"))
                    .GetType("jsreport.Embedded.EmbeddedReportingServer");

            _embeddedReportingServer = Activator.CreateInstance(embeddedReportingServerType, FindFreePort());

            _embeddedReportingServer.BinPath = _currentShadowBinFolder;
            await ((Task)_embeddedReportingServer.StartAsync()).ConfigureAwait(false);
        }

        private string GetMainModuleFilePath(int processId)
        {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (ManagementObjectCollection results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string) mo["ExecutablePath"] ?? string.Empty;
                    }
                }
            }
            return string.Empty;
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