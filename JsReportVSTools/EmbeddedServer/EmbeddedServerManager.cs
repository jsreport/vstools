using System;
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

namespace JsReportVSTools.EmbeddedServer
{
    public class EmbeddedServerManager
    {
        private AppDomain _domain;
        private dynamic _embeddedReportingServer;
        public string CurrentShadowBinFolder { get; set; }
        private string _currentBinFolder;
        public string CurrentProjectName { get; set; }
        private bool _running;
        private DTE2 _dte;

        public EmbeddedServerManager(DTE2 dte)
        {
            _dte = dte;
        }

        public string EmbeddedServerUri
        {
            get { return _embeddedReportingServer.EmbeddedServerUri; }
        }

        public async Task StopAsync()
        {
            await ((Task)_embeddedReportingServer.StopAsync()).ConfigureAwait(false);
        }

        private Project GetActiveProject(string fileName = null)
        {
            if (_dte.ActiveDocument != null)
                return _dte.ActiveDocument.ProjectItem.ContainingProject;

            return _dte.Solution.FindProjectItem(fileName).ContainingProject;
        }

        private static readonly object _locker = new object();
        public async Task EnsureStartedAsync(string fileName = null)
        {
            //lock (_locker)
            {
                if (_running &&
                    System.Diagnostics.Process.GetProcessesByName("node")
                        .Any(p => GetMainModuleFilePath(p.Id).Contains(CurrentShadowBinFolder)))
                    return;

                if (_running && CurrentProjectName != GetActiveProject(fileName).UniqueName)
                    await StopAsync().ConfigureAwait(false);

                await StartAsync(fileName).ConfigureAwait(false);
            }
        }

        private string GetMainModuleFilePath(int processId)
        {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return "";
        }

        private void Copy(string source, string destination, string pattern = "*.*")
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            foreach (string newPath in Directory.GetFiles(source, pattern, SearchOption.TopDirectoryOnly))
            {
                var destPath = newPath.Replace(source, destination);
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

        public async Task<object> SynchronizeTemplatesAsync()
        {
            Copy(_currentBinFolder, CurrentShadowBinFolder, "*.jsrep*");
            return await Task.Run( async () =>  
                await ((Task<object>)_embeddedReportingServer.SynchronizeLocalTemplatesAsync()).ConfigureAwait(false)).ConfigureAwait(false);
        }

        static long FindFreePort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public async Task StartAsync(string fileName = null)
        {
            var project = GetActiveProject(fileName);
            
            CurrentProjectName = project.UniqueName;
            
            _dte.Solution.SolutionBuild.BuildProject("Debug", project.UniqueName, true);

            _currentBinFolder = Path.Combine(new FileInfo(project.FullName).DirectoryName, "Bin", "Debug");
            string pathToDll = Path.Combine(_currentBinFolder, "jsreport.Embedded.dll");

            if (!File.Exists(pathToDll))
            {
                MessageBox.Show(
                    "jsreport.Embedded.dll was not found in path " + pathToDll +
                    " . Aren't you missing nuget package jsreport.Embedded?", "jsreport error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            CurrentShadowBinFolder = Path.Combine(Path.GetTempPath(), "jsreport-embedded-" + CurrentProjectName.Replace(".csproj", ""));
            Copy(_currentBinFolder, CurrentShadowBinFolder);

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

            System.Threading.Thread.Sleep(1000);
            _running = true;
        } 
    }
}