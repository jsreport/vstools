using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JsReportVSTools.Options
{
    [ComVisibleAttribute(true)]
    public class GeneralOptions : DialogPage
    {
        public GeneralOptions()
        {
            //Settings.Updated += delegate { LoadSettingsFromStorage(); };
        }

        //public override void SaveSettingsToStorage()
        //{
        //    Settings.SetValue(WESettings.Keys.EnableMustache, EnableMustache);
           

        //    Settings.Save();
        //}

        //public override void LoadSettingsFromStorage()
        //{
        //    EnableMustache = WESettings.GetBoolean(WESettings.Keys.EnableMustache);           
        //}
                
        [LocDisplayName("JsReporting service url")]
        [Description("Fill url where is javascript reporting service hosted.")]
        [Category("General")]
        public string ReportingServiceUrl { get; set; }
    }
}
