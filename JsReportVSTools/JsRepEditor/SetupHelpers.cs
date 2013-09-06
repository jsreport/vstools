﻿using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsReportVSTools.JsRepEditor
{
    public static class SetupHelpers
    {
        internal static Project GetActiveProject()
        {
            DTE dte = JsReportVSToolsPackage.GetGlobalService(typeof(SDTE)) as DTE;         
            return GetActiveProject(dte);
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

        public static string GetReportingServiceUrl()
        {
            DTE env = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE;

            EnvDTE.Properties props = env.get_Properties("JsReport", "General");
            
            return (string)props.Item("ReportingServiceUrl").Value;         
        }
      
        //public static void Foo()
        //{            
        //    WAProjectExtender extend = null;
                       

        //    foreach (object item in (Array)GetActiveProject().ExtenderNames)
        //    {
        //        extend = project.Extender[item.ToString()] as Microsoft.VisualStudio.Web.Application.WAProjectExtender;
        //        if (extend != null)
        //        {
        //            return extend.SilverlightDebugging;
        //        }
        //    }
        //}
    }
}
