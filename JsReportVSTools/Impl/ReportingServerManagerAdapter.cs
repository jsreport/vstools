using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace JsReportVSTools.Impl
{
    /// <summary>
    /// Proxy over a reporting server manager running in separate domain
    /// </summary>
    public class ReportingServerManagerAdapter
    {
        /* Communication with jsreport.Client and jsreport.Embedded is done in separate app domain to avoid dependency on particular version of dll
         * and also because we want to be able to unload domain and remove file locks on dll to allow user upgrading jsreport.Client version without
         * visual studio restart 
         */

        private DTE2 _dte;
        private dynamic _configuration;
        private IReportingServerManager _serverManager;
        private AppDomain _currentAppDomain;
        private bool _overwriteTemplatesDuringNextSync;
        private IEnumerable<string> _cachedRecipes;
        private IEnumerable<string> _cachedSchemas;
        private IEnumerable<string> _cachedEngines;
        private FileSystemWatcher _clientWatcher;

        public ReportingServerManagerAdapter(DTE2 dte)
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
            if (CurrentProject != null && GetActiveProject(fileName).UniqueName != GetCurrentProjectUniqueNameSafely())
            {
                _overwriteTemplatesDuringNextSync = true;
                await StopAsync();
            }

            _serverManager = _serverManager ?? CreateServerManager(fileName);

            try
            {
                await RemoteTask.ClientComplete(_serverManager.EnsureStartedAsync(fileName), CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                StopAsync();
                throw;
            }
        }

        //CurrentProject.UniqueName can throw Project unavailable exception
        private string GetCurrentProjectUniqueNameSafely()
        {
            try
            {
                return CurrentProject.UniqueName;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task StopAsync()
        {
            try
            {
                ClearCache();

                if (_serverManager != null)
                {
                    var tmpServerManager = _serverManager;
                    _serverManager = null;
                    await RemoteTask.ClientComplete(tmpServerManager.StopAsync(), CancellationToken.None).ConfigureAwait(false);
                }
            }
            //stopping does not need to go well 
            catch (Exception e)
            {
                
            }

        }

        public void ClearCache()
        {
            _cachedEngines = null;
            _cachedRecipes = null;
            _cachedSchemas = null;
        }

        private IReportingServerManager CreateServerManager(string fileName)
        {
            if (_currentAppDomain != null)
            {
                try
                {
                    AppDomain.Unload(_currentAppDomain);
                }
                catch (Exception e)
                {
                    //dont know how to check if its already unloaded
                }
            }

            CurrentProject = GetActiveProject(fileName);
            _dte.Solution.SolutionBuild.BuildProject("Debug", CurrentProject.UniqueName, true);
            

            if (!File.Exists(Path.Combine(CurrentBinFolder, "jsreport.Client.dll")))
            {
                throw new MissingJsReportDllException(
                    "Missing jsreport.Client.dll arent you missing nuget package? \n location: " + CurrentBinFolder);
            }


            if (_clientWatcher == null)
            {
                //upgrading jsreport.Client nuget should retart running reporting server manager
                _clientWatcher = new FileSystemWatcher(Path.Combine(CurrentBinFolder));
                _clientWatcher.Changed += (s, a) =>
                {
                    if (a.FullPath.Contains("jsreport.Client.dll"))
                        StopAsync().Wait();
                };

                _clientWatcher.EnableRaisingEvents = true;
            }

            //move everything to another folder so dll files locks are not blocking vs from build
            CopyToShadowFolder();

            _currentAppDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null, new AppDomainSetup
            {
                ApplicationBase = CurrentShadowBinFolder,
                PrivateBinPath = CurrentShadowBinFolder,
                LoaderOptimization = LoaderOptimization.MultiDomainHost,
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = "true"
            });

            var type = typeof(ReportingServerFactory);
            var factory = (IReportingServerFactory)_currentAppDomain.CreateInstanceFromAndUnwrap(type.Assembly.Location, type.FullName);
            return factory.Create(CurrentShadowBinFolder, CurrentProject.Name);
        }

        public async Task<IEnumerable<string>> GetRecipesAsync(string fileName)
        {
            if (_cachedRecipes == null)
            {
                await EnsureStartedAsync(fileName).ConfigureAwait(false);

                _cachedRecipes = await RemoteTask.ClientComplete(_serverManager.GetRecipesAsync(), CancellationToken.None).ConfigureAwait(false);
            }

            return _cachedRecipes;
        }

        public async Task<IEnumerable<string>> GetEnginesAsync(string fileName)
        {
            if (_cachedEngines == null)
            {
                await EnsureStartedAsync(fileName).ConfigureAwait(false);

                _cachedEngines = await RemoteTask.ClientComplete(_serverManager.GetEnginesAsync(), CancellationToken.None).ConfigureAwait(false);
            }

            return _cachedEngines;
        }

        public IEnumerable<string> GetSchemas()
        {
            if (_cachedSchemas != null)
                return _cachedSchemas;

            _dte.Solution.SolutionBuild.BuildProject("Debug", CurrentProject.UniqueName, true);
            var schemasFromFiles = Directory.GetFiles(CurrentBinFolder, "*.jsrep.json", SearchOption.AllDirectories)
                .Select(p => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(p))).Distinct();

            return _cachedSchemas = schemasFromFiles.Concat(_serverManager.Schemas).ToList();
        }

        public async Task<int> SynchronizeTemplatesAsync()
        {
            //we want to provide all the jsrep files into shadow bin folder where is actual jsreport.Client running

            FileSystemHelpers.DeleteFilesInDirectory(CurrentBinFolder, "*.jsrep*");
            FileSystemHelpers.DeleteFilesInDirectory(CurrentShadowBinFolder, "*.jsrep*");

            //problem is visual studio does not delete old files in bin folder when moving files between folders, therefor 
            //I need to take files from vs project and cannot rely on bin fodler
            var projectPath = new FileInfo(GetActiveProject().FullName).DirectoryName;

            foreach (var reportItem in GetActiveProject().GetAllProjectItems().Where(i => i.Name.Contains(".jsrep")))
            {
                var filePath = Path.Combine(new FileInfo(reportItem.FileNames[0]).DirectoryName, reportItem.Name);
                File.Copy(filePath, filePath.Replace(projectPath, CurrentBinFolder), true);
            }

            FileSystemHelpers.Copy(CurrentBinFolder, CurrentShadowBinFolder, "*.jsrep*", _overwriteTemplatesDuringNextSync);
            _overwriteTemplatesDuringNextSync = false;
            return await RemoteTask.ClientComplete(_serverManager.SynchronizeTemplatesAsync(), CancellationToken.None).ConfigureAwait(false);
        }

        private void CopyToShadowFolder()
        {
            CurrentShadowBinFolder = Path.Combine(Path.GetTempPath(), "jsreport-embedded");

            if (Directory.Exists(CurrentShadowBinFolder))
                FileSystemHelpers.DeleteDirectory(CurrentShadowBinFolder);
            
            FileSystemHelpers.Copy(CurrentBinFolder, CurrentShadowBinFolder);
        }

        public Project CurrentProject { get; set; }

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

        public async Task<object> RenderAsync(string shortid, string schemaName)
        {
            return await RemoteTask.ClientComplete(_serverManager.RenderAsync(shortid, schemaName), CancellationToken.None).ConfigureAwait(false);
        }
    }
}