using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace JsReportVSTools.Impl
{
    /// <summary>
    ///     Proxy over a reporting server manager running in separate domain
    /// </summary>
    public class ReportingServerManagerAdapter
    {
        /* Communication with jsreport.Client and jsreport.Embedded is done in separate app domain to avoid dependency on particular version of dll
         * and also because we want to be able to unload domain and remove file locks on dll to allow user upgrading jsreport.Client version without
         * visual studio restart 
         */

        private readonly DTE2 _dte;
        private readonly AsyncLock _lock = new AsyncLock();
        private IEnumerable<string> _cachedEngines;
        private IEnumerable<string> _cachedRecipes;
        private IEnumerable<string> _cachedSampleData;
        private FileSystemWatcher _clientWatcher;
        private dynamic _configuration;
        private AppDomain _currentAppDomain;
        private bool _overwriteTemplatesDuringNextSync;
        private IReportingServerManager _serverManager;

        public ReportingServerManagerAdapter(DTE2 dte)
        {
            _dte = dte;
        }

        public Project CurrentProject { get; set; }

        public string ServerUri
        {
            get { return _serverManager.ServerUri; }
        }

        public string CurrentShadowBinFolder { get; set; }

        public string CurrentBinFolder
        {
            get
            {
                string outputPath =
                    CurrentProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath")
                        .Value.ToString();
                return Path.Combine(new FileInfo(CurrentProject.FullName).DirectoryName, outputPath);
            }
        }

        private Project GetActiveProject(string fileName = null)
        {
            if (_dte.ActiveDocument != null)
                return _dte.ActiveDocument.ProjectItem.ContainingProject;

            return _dte.Solution.FindProjectItem(fileName).ContainingProject;
        }

        public async Task EnsureStartedAsync(string fileName = null)
        {
            using (await _lock.LockAsync())
            {
                try
                {
                    //we keep just one instance of the server manager per projec
                    if (CurrentProject != null &&
                        GetActiveProject(fileName).UniqueName != GetCurrentProjectUniqueNameSafely())
                    {
                        _overwriteTemplatesDuringNextSync = true;
                        await StopAsync();
                    }

                    Trace.WriteLine("Creating server");
                    _serverManager = _serverManager ?? CreateServerManager(fileName);
                    Trace.WriteLine("Created");

                    await
                        RemoteTask.ClientComplete(_serverManager.EnsureStartedAsync(), CancellationToken.None)
                            .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _serverManager = null;
                    Trace.TraceError("Failed to start jsreport server " + e);
                    throw;
                }
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
            using (await _lock.LockAsync())
            {
                Trace.WriteLine("Stopping server");
                try
                {
                    ClearCache();

                    if (_serverManager != null)
                    {
                        IReportingServerManager tmpServerManager = _serverManager;
                        _serverManager = null;
                        await
                            RemoteTask.ClientComplete(tmpServerManager.StopAsync(), CancellationToken.None)
                                .ConfigureAwait(false);
                    }
                }
                    //stopping does not need to go well 
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                }
                finally
                {
                    Trace.WriteLine("Stoppedr");
                }
            }
        }

        public void ClearCache()
        {
            _cachedEngines = null;
            _cachedRecipes = null;
            _cachedSampleData = null;
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

            _dte.Solution.SolutionBuild.BuildProject(_dte.Solution.SolutionBuild.ActiveConfiguration.Name,
                CurrentProject.UniqueName, true);
            if (_dte.Solution.SolutionBuild.LastBuildInfo > 0)
            {
                throw new WeakJsReportException("Fix build errors first");
            }


            if (!File.Exists(Path.Combine(CurrentBinFolder, "jsreport.Client.dll")))
            {
                throw new WeakJsReportException(
                    "Missing jsreport.Client.dll. Install jsreport.Embedded nuget package or install jsreport.Client and use ReportingStartup.cs to configure remote jsreport.");
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

            Type type = typeof (ReportingServerFactory);
            var factory =
                (IReportingServerFactory)
                    _currentAppDomain.CreateInstanceFromAndUnwrap(type.Assembly.Location, type.FullName);
            return factory.Create(CurrentShadowBinFolder, CurrentProject.Name);
        }

        public async Task<IEnumerable<string>> GetRecipesAsync(string fileName)
        {
            if (_cachedRecipes == null)
            {
                await EnsureStartedAsync(fileName).ConfigureAwait(false);

                _cachedRecipes =
                    await
                        RemoteTask.ClientComplete(_serverManager.GetRecipesAsync(), CancellationToken.None)
                            .ConfigureAwait(false);
            }

            return _cachedRecipes;
        }

        public async Task<IEnumerable<string>> GetEnginesAsync(string fileName)
        {
            if (_cachedEngines == null)
            {
                await EnsureStartedAsync(fileName).ConfigureAwait(false);

                _cachedEngines =
                    await
                        RemoteTask.ClientComplete(_serverManager.GetEnginesAsync(), CancellationToken.None)
                            .ConfigureAwait(false);
            }

            return _cachedEngines;
        }

        public async Task<IEnumerable<string>> GetSampleDataItems()
        {
            using (await _lock.LockAsync())
            {
                if (_cachedSampleData != null)
                    return _cachedSampleData;

                _dte.Solution.SolutionBuild.BuildProject(_dte.Solution.SolutionBuild.ActiveConfiguration.Name,
                    CurrentProject.UniqueName, true);
                IEnumerable<string> sampleDataFromFiles =
                    Directory.GetFiles(CurrentBinFolder, "*.jsrep.json", SearchOption.AllDirectories)
                        .Select(p => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(p))).Distinct();

                return _cachedSampleData = sampleDataFromFiles.Concat(_serverManager.SampleDataItems).ToList();
            }
        }

        public async Task<int> SynchronizeTemplatesAsync()
        {
            //we want to provide all the jsrep files into shadow bin folder where is actual jsreport.Client running

            FileSystemHelpers.DeleteFilesInDirectory(CurrentBinFolder, "*.jsrep*");
            FileSystemHelpers.DeleteFilesInDirectory(CurrentShadowBinFolder, "*.jsrep*");

            //problem is visual studio does not delete old files in bin folder when moving files between folders, therefor 
            //I need to take files from vs project and cannot rely on bin fodler
            string projectPath = new FileInfo(GetActiveProject().FullName).DirectoryName;

            foreach (
                ProjectItem reportItem in GetActiveProject().GetAllProjectItems().Where(i => i.Name.Contains(".jsrep")))
            {
                string filePath = Path.Combine(new FileInfo(reportItem.FileNames[0]).DirectoryName, reportItem.Name);
                File.Copy(filePath, filePath.Replace(projectPath, CurrentBinFolder), true);
            }

            FileSystemHelpers.Copy(CurrentBinFolder, CurrentShadowBinFolder, "*.jsrep*",
                _overwriteTemplatesDuringNextSync);
            _overwriteTemplatesDuringNextSync = false;
            return
                await
                    RemoteTask.ClientComplete(_serverManager.SynchronizeTemplatesAsync(), CancellationToken.None)
                        .ConfigureAwait(false);
        }

        private void CopyToShadowFolder()
        {
            CurrentShadowBinFolder = Path.Combine(Path.GetTempPath(), "jsreport-embedded");
            FileSystemHelpers.Copy(CurrentBinFolder, CurrentShadowBinFolder);
        }

        public object CreateReportingService()
        {
            return _serverManager.CreateReportingService();
        }

        public async Task<object> RenderAsync(string shortid, string sampleData)
        {
            return
                await
                    RemoteTask.ClientComplete(_serverManager.RenderAsync(shortid, sampleData), CancellationToken.None)
                        .ConfigureAwait(false);
        }
    }
}