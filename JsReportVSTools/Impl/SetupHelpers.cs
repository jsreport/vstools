using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace JsReportVSTools.Impl
{
    public static class SetupHelpers
    {
        private static DTE2 _dte;
        private static CommandEvents _debugCommandEvent;
        private static CommandEvents _cteateProjectItemEvent;
        private static DocumentEvents _documentEvents;
        public static ReportingServerManagerAdapter ReportingServerManagerAdapter { get; set; }

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

            //new project item should clear schemas cache, so new jsrep.json is visible in combo
            _cteateProjectItemEvent = _dte.Events.CommandEvents["{5EFC7975-14BC-11CF-9B2B-00AA00573819}", 220];
            _cteateProjectItemEvent.AfterExecute +=
                (string guid, int id, object @in, object @out) => ReportingServerManagerAdapter.ClearCache();

            //saving ReportingStartup.cs should restart current server manager
            _documentEvents = _dte.Events.DocumentEvents;
            _documentEvents.DocumentSaved += async document =>
            {
                if (document.FullName.ToLower().Contains("reportingstartup"))
                {
                    await ReportingServerManagerAdapter.StopAsync();
                }
            };

            ReportingServerManagerAdapter = new ReportingServerManagerAdapter(_dte);
        }

        private static async Task DoPreviewActiveItemInner(CommonMessagePump msgPump)
        {
            try
            {
                msgPump.TotalSteps = 5;
                msgPump.CurrentStep = 1;
                msgPump.ProgressText = "Preparing jsreport server";
                msgPump.StatusBarText = msgPump.ProgressText;

                await ReportingServerManagerAdapter.EnsureStartedAsync().ConfigureAwait(false);

                msgPump.CurrentStep = 2;
                msgPump.ProgressText = "Synchronizing templates";
                msgPump.StatusBarText = msgPump.ProgressText;

                _dte.ExecuteCommand("File.SaveAll");

                _dte.Solution.SolutionBuild.BuildProject("Debug", ReportingServerManagerAdapter.CurrentProject.UniqueName, true);

                await ReportingServerManagerAdapter.SynchronizeTemplatesAsync().ConfigureAwait(false);

                msgPump.CurrentStep = 3;
                msgPump.ProgressText = "Rendering template in jsreport";
                msgPump.StatusBarText = msgPump.ProgressText;


                string definitionPath = _dte.ActiveDocument.FullName.RemoveFromEnd(".html").RemoveFromEnd(".js");
                var rd = ReadReportDefinition(definitionPath);

                //jsreport shortid is case sensitive and _dte.ActiveDocument.Name sometime does not return exact filename value
                var shortid = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(new FileInfo(_dte.ActiveDocument.Name).Name));
                dynamic report = await ReportingServerManagerAdapter.RenderAsync(shortid, rd.Schema);

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
            catch (MissingJsReportDllException e)
            {
                MessageBox.Show(e.Message, "jsreport error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error when processing template", MessageBoxButtons.OK,
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
            return ReportingServerManagerAdapter.ServerUri;
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

            var item = _dte.ActiveDocument.ProjectItem.ContainingProject.GetAllProjectItems().SingleOrDefault(p => p.Name == fileNameToSearch);

            if (item != null)
            {
                Window window = item.Open();
                window.Activate();
            }
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

        public static Task<IEnumerable<string>> GetRecipesAsync(string fileName)
        {
            return ReportingServerManagerAdapter.GetRecipesAsync(fileName);
        }

        public static IEnumerable<string> GetSchemas()
        {
            return ReportingServerManagerAdapter.GetSchemas();
        }

        public static Task<IEnumerable<string>> GetEnginesAsync(string fileName)
        {
            return ReportingServerManagerAdapter.GetEnginesAsync(fileName);
        }

        public static void OpenEmbeddedServer()
        {
            System.Diagnostics.Process.Start(ReportingServerManagerAdapter.ServerUri);
        }

        private static string RemoveFromEnd(this string s, string suffix)
        {
            if (s.EndsWith(suffix))
            {
                return s.Substring(0, s.Length - suffix.Length);
            }
            return s;
        }
    }
}