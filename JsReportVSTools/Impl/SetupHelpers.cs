using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;

namespace JsReportVSTools.Impl
{
    public static class SetupHelpers
    {
        private static DTE2 _dte;
        private static IEnumerable<string> _cachedRecipes;
        private static IEnumerable<string> _cachedSchemas;
        private static IEnumerable<string> _cachedEngines;
        private static CommandEvents _debugCommandEvent;
        private static CommandEvents _cteateProjectItemEvent;
        public static EmbeddedServerManager EmbeddedServerManager { get; set; }

        public static bool HasCachedEngines
        {
            get { return _cachedEngines != null; }
        }

        public static bool HasCachedRecipes
        {
            get { return _cachedRecipes != null; }
        }

        public static void InitializeListeners()
        {
            _dte = ServiceProvider.GlobalProvider.GetService(typeof (DTE)) as DTE2;

            //register for F5 hit event and start previewing if selected document is jsreport
            _debugCommandEvent = _dte.Events.CommandEvents["{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 295];
            _debugCommandEvent.BeforeExecute +=
                (string guid, int id, object @in, object @out, ref bool @default) =>
                {
                    if (_dte.ActiveDocument != null && _dte.ActiveDocument.FullName.Contains(".jsrep") && 
                        !_dte.ActiveDocument.FullName.Contains(".png") &&
                        !_dte.ActiveDocument.FullName.Contains(".json"))
                    {
                        @default = true;
                        DoPreviewActiveItem().Wait();
                    }
                };

            _cteateProjectItemEvent = _dte.Events.CommandEvents["{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 220];

            _cteateProjectItemEvent.AfterExecute +=
                (string guid, int id, object @in, object @out) =>
                {
                    _cachedSchemas = null;
                };

            EmbeddedServerManager = new EmbeddedServerManager(_dte);
        }

        private static async Task DoPreviewActiveItemInner(CommonMessagePump msgPump)
        {
            try
            {
                msgPump.TotalSteps = 5;
                msgPump.CurrentStep = 1;
                msgPump.ProgressText = "Preparing jsreport server";
                msgPump.StatusBarText = msgPump.ProgressText;

                await EmbeddedServerManager.EnsureStartedAsync().ConfigureAwait(false);

                msgPump.CurrentStep = 2;
                msgPump.ProgressText = "Synchronizing templates";
                msgPump.StatusBarText = msgPump.ProgressText;

                _dte.ExecuteCommand("File.SaveAll");

                _dte.Solution.SolutionBuild.BuildProject("Debug", EmbeddedServerManager.CurrentProjectName, true);
                
                await EmbeddedServerManager.SynchronizeTemplatesAsync().ConfigureAwait(false);

                msgPump.CurrentStep = 3;
                msgPump.ProgressText = "Rendering template in jsreport";
                msgPump.StatusBarText = msgPump.ProgressText;

                dynamic service = EmbeddedServerManager.CreateReportingService();
                
                //jsreport shortid is case sensitive and _dte.ActiveDocument.Name sometime does not return exact filename value
                var shortid = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(new FileInfo(_dte.ActiveDocument.Name).Name));
                dynamic report = await service.RenderAsync(shortid, null);

                msgPump.CurrentStep = 4;
                msgPump.ProgressText = "Opening report";
                msgPump.StatusBarText = msgPump.ProgressText;

                string tempFile = Path.GetTempFileName();
                tempFile = Path.ChangeExtension(tempFile, report.FileExtension);

                using (FileStream fileStream = File.Create(tempFile))
                {
                    report.Content.CopyTo(fileStream);
                }

                OpenFileInBrowser(tempFile);
            }
            catch (MissingJsReportEmbeddedDllException e)
            {
                MessageBox.Show(e.Message, "jsreport error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                dynamic ed = e;
                //is it a jsrepo
                var message = e.GetType().Name == "JsReportException" ? ed.ResponseErrorMessage : e.ToString();
                MessageBox.Show(message, "Error when processing template", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public static Task DoPreviewActiveItem()
        {
            var msgPump = new CommonMessagePump();
            msgPump.AllowCancel = false;
            msgPump.EnableRealProgress = true;
            msgPump.WaitTitle = "Rendering report";
            msgPump.WaitText = "This should NOT take several minutes. :)";

            var task = Task.Run(async () => await DoPreviewActiveItemInner(msgPump));
            msgPump.ModalWaitForHandles(((IAsyncResult)task).AsyncWaitHandle);
            return task;
        }

        public static ReportDefinition ReadReportDefinition(string fileName)
        {
            var s = new XmlSerializer(typeof (ReportDefinition));
            return s.Deserialize(new StreamReader(fileName)) as ReportDefinition;
        }

        public static string GetReportingServiceUrl()
        {
            return EmbeddedServerManager.EmbeddedServerUri;
        }

        public static void OpenHelpers()
        {
            ProjectItem item = _dte.Solution.FindProjectItem(_dte.ActiveDocument.FullName.Replace(".jsrep", ".jsrep.js"));
            Window window = item.Open();
            window.Activate();
        }

        public static void OpenHtml()
        {
            ProjectItem item =
                _dte.Solution.FindProjectItem(_dte.ActiveDocument.FullName.Replace(".jsrep", ".jsrep.html"));
            Window window = item.Open();
            window.Activate();
        }

        public static void OpenSchema(ReportDefinition rd)
        {
            var fileNameToSearch = rd.Schema + ".jsrep.json";

            var item = GetAllProjectItemsFromActiveProject().SingleOrDefault(p => p.Name == fileNameToSearch);

            if (item != null)
            {
                Window window = item.Open();
                window.Activate();
            }
        }

        private static IEnumerable<ProjectItem> GetAllProjectItemsFromActiveProject()
        {
            var rootItems = _dte.ActiveDocument.ProjectItem.ContainingProject.ProjectItems.Cast<ProjectItem>().ToList();
            var result = new List<ProjectItem>(rootItems);


            foreach (var item in rootItems)
            {
                 result.AddRange(GetAllProjectItemsFromActiveProjectInner(item));
            }

            return result;
        } 

        private static IEnumerable<ProjectItem> GetAllProjectItemsFromActiveProjectInner(ProjectItem item)
        {
            var result = new List<ProjectItem>() {item};

            foreach (ProjectItem innerItem in item.ProjectItems)
            {
                result.AddRange(GetAllProjectItemsFromActiveProjectInner(innerItem));    
            }

            return result;
        } 

        public static void OpenFileInBrowser(string path)
        {
            _dte.ExecuteCommand("View.WebBrowser", "\"file:///" + path.Replace('\\', '/') + "\"");
        }

        public static string ReadFile(string pathInSolution)
        {
            string rootDir = new FileInfo(_dte.Solution.FullName).Directory.FullName;

            return File.ReadAllText(Path.Combine(rootDir, pathInSolution));
        }

        public static string GetSolutionFileFullName(string fullPath)
        {
            string rootDir = new FileInfo(_dte.Solution.FullName).Directory.FullName;
            return fullPath.ToLower().Replace(rootDir.ToLower() + "\\", "");
        }

        public static void TryCloseActiveDocument()
        {
            try
            {
                _dte.ActiveDocument.Close();
            }
            catch (Exception e)
            {
                
            }
        }

        public static async Task<IEnumerable<string>> GetRecipesAsync(string fileName)
        {
            if (_cachedRecipes == null)
            {
                await EmbeddedServerManager.EnsureStartedAsync(fileName);

                dynamic service = EmbeddedServerManager.CreateReportingService();

                _cachedRecipes = await service.GetRecipesAsync();
            }

            return _cachedRecipes;
        }

        public static IEnumerable<string> GetSchemas()
        {
            if (_cachedSchemas != null)
                return _cachedSchemas;

            _dte.Solution.SolutionBuild.BuildProject("Debug", EmbeddedServerManager.CurrentProjectName, true);
            return _cachedSchemas = Directory.GetFiles(EmbeddedServerManager.CurrentBinFolder, "*.jsrep.json", SearchOption.AllDirectories)
                .Select(p => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(p)));
        }

        public static async Task<IEnumerable<string>> GetEngines(string fileName)
        {
            if (_cachedEngines == null)
            {
                var msgPump = new CommonMessagePump();
                msgPump.AllowCancel = false;
                msgPump.EnableRealProgress = true;
                msgPump.WaitTitle = "Rendering report";
                msgPump.WaitText = "This should NOT take several minutes. :)";

                var task = EmbeddedServerManager.EnsureStartedAsync(fileName);
                msgPump.ModalWaitForHandles(((IAsyncResult)task).AsyncWaitHandle);

                await task;
                
                dynamic service = EmbeddedServerManager.CreateReportingService();
                _cachedEngines = await service.GetEnginesAsync();

                return _cachedEngines;
            }
            
            return _cachedEngines;
        }

        public static void OpenEmbeddedServer()
        {
            System.Diagnostics.Process.Start(EmbeddedServerManager.EmbeddedServerUri);
        }
    }
}