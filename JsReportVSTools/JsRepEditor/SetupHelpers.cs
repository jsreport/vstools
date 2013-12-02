using EnvDTE;
using JsReport;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JsReportVSTools.JsRepEditor
{
    public static class SetupHelpers
    {
        private static DTE _dte;

        static SetupHelpers() {
            _dte = JsReportVSToolsPackage.GetGlobalService(typeof(SDTE)) as DTE;
        }

        public static IEnumerable<string> Foo()
        {
            var rd = ReadReportDefinition(_dte.ActiveDocument.FullName.Replace("html", ""));

            var json =  JObject.Parse(ReadFile(rd.SchemaPath));

            var result = new List<string>();
            ParseJsonAttributes("", json, result);

            return result;
        }

        private static void ParseJsonAttributes(string path, JObject obj, List<string> attrs)
        {
            foreach (var property in  obj.Properties())
            {
                var current = path + "." + property.Name;
                current = current.TrimStart('.');
                attrs.Add(current);

                if (property.Value.Type == JTokenType.Object)
                    ParseJsonAttributes(current, (JObject)property.Value, attrs);
            }
        }

        public static ReportDefinition ReadReportDefinition(string fileName)
        {
            try
            {
                var s = new XmlSerializer(typeof(ReportDefinition));
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
            EnvDTE.Properties props = _dte.get_Properties("JsReport", "General");
            
            return (string)props.Item("ReportingServiceUrl").Value;         
        }

        public static void SetReportingServiceUrl(string url)
        {
            EnvDTE.Properties props = _dte.get_Properties("JsReport", "General");

            props.Item("ReportingServiceUrl").Value = url;
        }

        public static void OpenHelpers()
        {
            var item = _dte.Solution.FindProjectItem(_dte.ActiveDocument.FullName.Replace(".jsrep", ".jsrep.js"));
            var window = item.Open(EnvDTE.Constants.vsViewKindPrimary);
            window.Activate();
        }

        public static void OpenHtml()
        {
            var item = _dte.Solution.FindProjectItem(_dte.ActiveDocument.FullName.Replace(".jsrep", ".jsrep.html"));
            var window = item.Open(EnvDTE.Constants.vsViewKindPrimary);
            window.Activate();
        }

        public static string ReadFile(string pathInSolution)
        {
            var rootDir = new FileInfo(_dte.Solution.FullName).Directory.FullName;

            return File.ReadAllText(Path.Combine(rootDir, pathInSolution));           
        }

        public static string GetSolutionFileFullName(string fullPath)
        {
            var rootDir = new FileInfo(_dte.Solution.FullName).Directory.FullName;
            return fullPath.ToLower().Replace(rootDir.ToLower() + "\\", "");            
        }

        public static async Task<Report> RenderPreviewAsync(object data, string html, string helpers, string engine, string recipe)
        {
            var service = new ReportingService(GetReportingServiceUrl());

            return await service.RenderPreviewAsync(new RenderRequest()
            {
                Data = data,
                Template = new Template()
                {
                    Html = html,
                    Helpers = helpers,
                    Engine = engine
                },
                Options = new RenderOptions() { Async = false, Recipe = recipe }
            });
        }

        public static async Task<bool> ValidateServerUrlAsync(string url)
        {
            try
            {
                var service = new ReportingService(url);

                var version = await service.GetServerVersionAsync();

                return double.Parse(version, CultureInfo.InvariantCulture) > 0;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static async Task<IEnumerable<string>> GetRecipesAsync()
        {          
            var service = new ReportingService(GetReportingServiceUrl());

            return await service.GetRecipesAsync();
        }

        public static async Task<IEnumerable<string>> GetEngines()
        {
            var service = new ReportingService(GetReportingServiceUrl());

            return await service.GetEnginesAsync();
        }

        internal static Project GetActiveProject()
        {
            return GetActiveProject(_dte);
        }

        internal static Project GetActiveProject(DTE dte)
        {
            Project activeProject = null;

            Array activeSolutionProjects = dte.ActiveSolutionProjects as Array;
            if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
            {
                activeProject = activeSolutionProjects.GetValue(0) as Project;
            }

            return activeProject;
        }
    }
}
