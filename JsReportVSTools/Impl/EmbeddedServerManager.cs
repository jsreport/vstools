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
    public class EmbeddedServerManager
    {
        private readonly DTE2 _dte;
        private string _currentBinFolder;
        private dynamic _embeddedReportingServer;
        private bool _running;
        
        public string CurrentShadowBinFolder { get; set; }
        public string CurrentProjectName { get; set; }
        public string EmbeddedServerUri
        {
            get { return _embeddedReportingServer.EmbeddedServerUri; }
        }

        public EmbeddedServerManager(DTE2 dte)
        {
            _dte = dte;
        }

        private Project GetActiveProject(string fileName = null)
        {
            if (_dte.ActiveDocument != null)
                return _dte.ActiveDocument.ProjectItem.ContainingProject;

            return _dte.Solution.FindProjectItem(fileName).ContainingProject;
        }

        /// <summary>
        /// Ensures that jsreport server is running for current project
        /// </summary>
        public async Task EnsureStartedAsync(string fileName = null)
        {
            if (_running &&
                Process.GetProcessesByName("node")
                    .Any(p => GetMainModuleFilePath(p.Id).Contains(CurrentShadowBinFolder)))
                return;

            if (_running && CurrentProjectName != GetActiveProject(fileName).UniqueName)
                await StopAsync().ConfigureAwait(false);

            await StartAsync(fileName).ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronize jsreport templates from current project with currently running jsreport server
        /// </summary>
        public async Task SynchronizeTemplatesAsync()
        {
           Copy(_currentBinFolder, CurrentShadowBinFolder, "*.jsrep*");
           await Task.Run(async () => await (Task)_embeddedReportingServer.SynchronizeTemplatesAsync()).ConfigureAwait(false);
        }
        
        public IEnumerable<string> GetSchemas()
        {
            return Directory.EnumerateFiles(_currentBinFolder, "*.jsrep.json").Select(p => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(p)));
        }

        /// <summary>
        /// Creates dynamicly reporting service from bin folder
        /// </summary>
        public object CreateReportingService()
        {
            Type reportingServiceType =
                Assembly.LoadFrom(Path.Combine(CurrentShadowBinFolder, "jsreport.Client.dll"))
                    .GetType("jsreport.Client.ReportingService");

            return Activator.CreateInstance(reportingServiceType, EmbeddedServerUri);
        }
      
        private async Task StartAsync(string fileName = null)
        {
            /* We need to copy jsreport embedded dlls together with zipped server to temporary location
             * because windows will lock these files and we don't want to block visual studio from build.
             * We keep separate temporary directory for every project.
            */
            
            Project project = GetActiveProject(fileName);

            CurrentProjectName = project.UniqueName;

            _dte.Solution.SolutionBuild.BuildProject("Debug", project.UniqueName, true);

            _currentBinFolder = Path.Combine(new FileInfo(project.FullName).DirectoryName, "Bin", "Debug");
            string pathToDll = Path.Combine(_currentBinFolder, "jsreport.Embedded.dll");

            if (!File.Exists(pathToDll))
            {
                throw new MissingJsReportEmbeddedDllException("jsreport.Embedded.dll was not found in path " + pathToDll +
                                                              " . Aren't you missing nuget package jsreport.Embedded?");
                
            }

            CurrentShadowBinFolder = Path.Combine(Path.GetTempPath(),
                "jsreport-embedded-" + CurrentProjectName.Replace(".csproj", ""));
            Copy(_currentBinFolder, CurrentShadowBinFolder);


            if (Directory.Exists(Path.Combine(CurrentShadowBinFolder, "jsreport-net-embedded")))
                Directory.Delete(Path.Combine(CurrentShadowBinFolder, "jsreport-net-embedded"), true);

            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, args) =>
                {
                    return
                        Assembly.LoadFrom(Path.Combine(CurrentShadowBinFolder,
                            args.Name.Remove(args.Name.IndexOf(',')) + ".dll"));
                };

            Type embeddedReportingServerType =
                Assembly.LoadFrom(Path.Combine(CurrentShadowBinFolder, "jsreport.Embedded.dll"))
                    .GetType("jsreport.Embedded.EmbeddedReportingServer");

            _embeddedReportingServer = Activator.CreateInstance(embeddedReportingServerType, FindFreePort());

            _embeddedReportingServer.BinPath = CurrentShadowBinFolder;
            await ((Task)_embeddedReportingServer.StartAsync()).ConfigureAwait(false);

            _running = true;
        }

        private async Task StopAsync()
        {
            await ((Task)_embeddedReportingServer.StopAsync()).ConfigureAwait(false);
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

        private void Copy(string source, string destination, string pattern = "*.*")
        {
            if (Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            foreach (string newPath in Directory.GetFiles(source, pattern, SearchOption.TopDirectoryOnly))
            {
                string destPath = newPath.Replace(source, destination);
                try
                {
                    if (new FileInfo(destPath).LastWriteTime < new FileInfo(newPath).LastWriteTime)
                        File.Copy(newPath, destPath, true);
                }
                catch (Exception e)
                {
                }
            }
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