using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace JsReportVSTools.Impl
{
    public class ReportingServerAdapter
    {
        private DTE2 _dte;
        private dynamic _configuration;
        private IReportingServerManager _serverManager;

        public ReportingServerAdapter(DTE2 dte)
        {
            _dte = dte;
        }

        private Project GetActiveProject(string fileName = null)
        {
            if (_dte.ActiveDocument != null)
                return _dte.ActiveDocument.ProjectItem.ContainingProject;

            return _dte.Solution.FindProjectItem(fileName).ContainingProject;
        }

        public async Task EnsureStartedAsync(string fileName = null)
        {
            //we keep just one instance of the server manager per projec
            if (CurrentProject != null && GetActiveProject(fileName).UniqueName != CurrentProject.UniqueName)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ShadowBinAssemblyResolve;
                await _serverManager.StopAsync().ConfigureAwait(false);
                _serverManager = null;
            }

            _serverManager = _serverManager ?? CreateServerManager(fileName);

            try
            {
                await _serverManager.EnsureStartedAsync(fileName).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _serverManager = null;
                throw;
            }
        }

        public Task StopAsync()
        {
            if (_serverManager != null)
                return _serverManager.StopAsync();

            return Task.FromResult(0);
        }

        private IReportingServerManager CreateServerManager(string fileName)
        {
            CurrentProject = GetActiveProject(fileName);
            _dte.Solution.SolutionBuild.BuildProject("Debug", CurrentProject.UniqueName, true);

            CopyToShadowFolder();

            var dllName = Path.GetFileName(GetActiveProject(fileName).FullName.Replace(CurrentBinFolder, CurrentShadowBinFolder)).Replace(".csproj", "");

            dllName += File.Exists(Path.Combine(CurrentShadowBinFolder, dllName + ".dll")) ? ".dll" : ".exe";

            Type startupType =
                Assembly.LoadFrom(Path.Combine(CurrentShadowBinFolder, dllName))
                    .GetTypes()
                    .FirstOrDefault(t => t.Name.Contains("ReportingStartup"));

            if (startupType == null)
                return CreateEmbeddedServerManager();

            dynamic startup = Activator.CreateInstance(startupType);

            Type configurationType = Assembly.LoadFrom(Path.Combine(CurrentShadowBinFolder, "jsreport.Client.dll")).GetType("jsreport.Client.VSConfiguration.VSReportingConfiguration");

            _configuration = Activator.CreateInstance(configurationType);
            startup.Configure(_configuration);

            if (string.IsNullOrEmpty(_configuration.RemoteServerUri))
                return CreateEmbeddedServerManager();

            return new RemoteServerManager(_dte, CurrentShadowBinFolder, _configuration);
        }

        private EmbeddedServerManager CreateEmbeddedServerManager()
        {
            return new EmbeddedServerManager(_dte, CurrentProject, CurrentShadowBinFolder);
        }

        private void CopyToShadowFolder()
        {
            CurrentShadowBinFolder = Path.Combine(Path.GetTempPath(), "jsreport-embedded-" + CurrentProject.UniqueName.Replace(".csproj", ""));

            if (Directory.Exists(CurrentShadowBinFolder)) 
                Directory.Delete(CurrentShadowBinFolder, true);
            
            FileSystemHelpers.Copy(CurrentBinFolder, CurrentShadowBinFolder);

            AppDomain.CurrentDomain.AssemblyResolve += ShadowBinAssemblyResolve;
        }

        private Assembly ShadowBinAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFrom(Path.Combine(CurrentShadowBinFolder, args.Name.Remove(args.Name.IndexOf(',')) + ".dll"));
        }

        public Project CurrentProject { get; set; }

        public async Task SynchronizeTemplatesAsync()
        {
            FileSystemHelpers.Copy(CurrentBinFolder, CurrentShadowBinFolder, "*.jsrep*");
            dynamic rs = _serverManager.CreateReportingService();
            await Task.Run(async () => await (Task)rs.SynchronizeTemplatesAsync()).ConfigureAwait(false);
        }

        public object CreateReportingService()
        {
            return _serverManager.CreateReportingService();
        }

        public string ServerUri
        {
            get { return _serverManager.ServerUri; }
        }

        public string CurrentShadowBinFolder { get; set; }

        public string CurrentBinFolder
        {
            get { return Path.Combine(new FileInfo(CurrentProject.FullName).DirectoryName, "Bin", "Debug"); }
        }
    }
}