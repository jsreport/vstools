using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using EnvDTE;
using EnvDTE80;
using jsreport.Client;
using JsReportVSTools.EmbeddedServer;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;

namespace JsReportVSTools.JsRepEditor
{
    public static class SetupHelpers
    {
        private static DTE2 _dte;
        private static IEnumerable<string> _cachedRecipes;
        private static IEnumerable<string> _cachedEngines;
        private static CommandEvents _debugCommandEvents;
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

            _debugCommandEvents = _dte.Events.CommandEvents["{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 295];
            _debugCommandEvents.BeforeExecute +=
                (string guid, int id, object @in, object @out, ref bool @default) =>
                {
                    if (_dte.ActiveDocument != null && _dte.ActiveDocument.FullName.Contains(".jsrep"))
                    {
                        @default = true;
                        DoPreviewActiveItem();
                    }
                };

            EmbeddedServerManager = new EmbeddedServerManager(_dte);
        }

        public static string RemoveFromEnd(this string s, string suffix)
        {
            if (s.EndsWith(suffix))
            {
                return s.Substring(0, s.Length - suffix.Length);
            }
            return s;
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

                string definitionPath = _dte.ActiveDocument.FullName.RemoveFromEnd(".html").RemoveFromEnd(".js");

                ReportDefinition rd = ReadReportDefinition(definitionPath);

                await EmbeddedServerManager.SynchronizeTemplatesAsync().ConfigureAwait(false);

                object data = null;

                if (!string.IsNullOrEmpty(rd.SchemaPath))
                {
                    try
                    {
                        data = JObject.Parse(ReadFile(rd.SchemaPath));
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException("Unable to parse schema file.", e);
                    }
                }

                msgPump.CurrentStep = 3;
                msgPump.ProgressText = "Rendering template in jsreport";
                msgPump.StatusBarText = msgPump.ProgressText;

                Report report =
                    await
                        RenderPreviewAsync(
                            Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(_dte.ActiveDocument.Name)),
                            data);

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
            catch (JsReportException e)
            {
                MessageBox.Show(e.ResponseErrorMessage, "Error when processing template", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error when processing template", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public static void DoPreviewActiveItem()
        {
            var msgPump = new CommonMessagePump();
            msgPump.AllowCancel = false;
            msgPump.EnableRealProgress = true;
            msgPump.WaitTitle = "Rendering report";
            msgPump.WaitText = "This should NOT take several minutes. :)";

            var task = DoPreviewActiveItemInner(msgPump);
            msgPump.ModalWaitForHandles(((IAsyncResult)task).AsyncWaitHandle);

            task.Wait();
        }

        public static IEnumerable<string> Foo()
        {
            ReportDefinition rd = ReadReportDefinition(_dte.ActiveDocument.FullName.Replace("html", ""));

            JObject json = JObject.Parse(ReadFile(rd.SchemaPath));

            var result = new List<string>();
            ParseJsonAttributes("", json, result);

            return result;
        }

        private static void ParseJsonAttributes(string path, JObject obj, List<string> attrs)
        {
            foreach (JProperty property in  obj.Properties())
            {
                string current = path + "." + property.Name;
                current = current.TrimStart('.');
                attrs.Add(current);

                if (property.Value.Type == JTokenType.Object)
                    ParseJsonAttributes(current, (JObject) property.Value, attrs);
            }
        }

        public static ReportDefinition ReadReportDefinition(string fileName)
        {
            try
            {
                var s = new XmlSerializer(typeof (ReportDefinition));
                return s.Deserialize(new StreamReader(fileName)) as ReportDefinition;
            }
            catch (Exception e)
            {
                return new ReportDefinition();

                //we will recover with defaults
            }
        }

        public static string GetReportingServiceUrl()
        {
            return EmbeddedServerManager.EmbeddedServerUri;
        }

        public static void SetReportingServiceUrl(string url)
        {
            Properties props = _dte.get_Properties("JsReport", "General");

            props.Item("ReportingServiceUrl").Value = url;
        }

        public static void OpenHelpers()
        {
            ProjectItem item = _dte.Solution.FindProjectItem(_dte.ActiveDocument.FullName.Replace(".jsrep", ".jsrep.js"));
            Window window = item.Open(Constants.vsViewKindPrimary);
            window.Activate();
        }

        public static void OpenHtml()
        {
            ProjectItem item =
                _dte.Solution.FindProjectItem(_dte.ActiveDocument.FullName.Replace(".jsrep", ".jsrep.html"));
            Window window = item.Open(Constants.vsViewKindPrimary);
            window.Activate();
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

        public static async Task<Report> RenderPreviewAsync(string shortid, object data)
        {
            EmbeddedServerManager.EnsureStartedAsync();

            var service = new ReportingService(GetReportingServiceUrl());

            Report result = await service.RenderAsync(shortid, data);

            return result;
        }

        public static async Task<bool> ValidateServerUrlAsync(string url)
        {
            try
            {
                var service = new ReportingService(url);

                string version = await service.GetServerVersionAsync();

                return !string.IsNullOrEmpty(version);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static async Task<IEnumerable<string>> GetRecipesAsync(string fileName)
        {
            if (_cachedRecipes == null)
            {
                EmbeddedServerManager.EnsureStartedAsync(fileName);

                var service = new ReportingService(GetReportingServiceUrl());

                _cachedRecipes = await service.GetRecipesAsync();
            }

            return _cachedRecipes;
        }

        public static async Task<IEnumerable<string>> GetEngines(string fileName)
        {
            if (_cachedEngines == null)
            {
                var msgPump = new CommonMessagePump();
                msgPump.AllowCancel = false;
                msgPump.WaitTitle = "Initializing jsreport for first use";
                msgPump.WaitText = "This should NOT take several minutes. :)";

                var task = EmbeddedServerManager.EnsureStartedAsync(fileName);
                msgPump.ModalWaitForHandles(((IAsyncResult)task).AsyncWaitHandle);

                task.Wait();

                var service = new ReportingService(GetReportingServiceUrl());

                _cachedEngines = await service.GetEnginesAsync();
                return _cachedEngines;
            }


            return _cachedEngines;
        }

        internal static Project GetActiveProject()
        {
            return GetActiveProject(_dte);
        }

        internal static Project GetActiveProject(DTE2 dte)
        {
            Project activeProject = null;

            var activeSolutionProjects = dte.ActiveSolutionProjects as Array;
            if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
            {
                activeProject = activeSolutionProjects.GetValue(0) as Project;
            }

            return activeProject;
        }
    }
}