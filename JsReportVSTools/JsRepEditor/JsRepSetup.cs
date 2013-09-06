using System;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using JsReportVSTools;
using tom;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Serialization;
using System.Text;
using JsReport;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using JsReportVSTools.JsRepEditor;
using System.Collections.Generic;

namespace JsReportVSTools
{
    public partial class JsRepSetup : UserControl
    {
        private ReportDefinition _state = new ReportDefinition();
        private string _fileName;

        public JsRepSetup()
        {
            InitializeComponent();

            cbEngine.Items.Add(new { Text = "handlebars", Value = "handlebars" });
            cbEngine.Items.Add(new { Text = "jsrender", Value = "jsrender" });

            cbEngine.ValueMember = "Value";
            cbEngine.DisplayMember = "Text";
        }            

        public void LoadFile(string fileName)
        {
            _fileName = fileName;

            try {
                var s = new XmlSerializer(typeof(ReportDefinition));
                _state = s.Deserialize(new StreamReader(fileName)) as ReportDefinition;
            
                RefreshView();
            }
            catch(Exception e) {
            }
        }

        public void SaveFile(string fileName)
        {            
            var sb = new StringBuilder();
            var ms = new StringWriter(sb);

            var s = new XmlSerializer(typeof(ReportDefinition));            
            s.Serialize(ms, _state);

            File.WriteAllText(fileName, sb.ToString());
        }

        public bool ReadOnly { get; set; }

        public event EventHandler StateChanged;

        private void JsRepSetup_Load(object sender, EventArgs e)
        {
            RefreshView();
        }
    
        private void RefreshView()
        {
            tbTimeout.Text = _state.Timeout;
            cbEngine.Text = _state.TemplatingEngine;

            if (string.IsNullOrEmpty(_state.SchemaPath)) {
                lblSchemaFilePath.Hide();
             }
            else {
                lblSchemaFilePath.Text = Path.GetFileName(_state.SchemaPath);
                lblSchemaFilePath.Show();
            }         
        }
  
        private void tbTimeout_TextChanged(object sender, EventArgs e)
        {
           _state.Timeout = tbTimeout.Text;
           NotifyChange();
        }

        private void openSchemaDialog_click(object sender, EventArgs e)
        {
            schemaDialog.ShowDialog();

            if (string.IsNullOrEmpty(schemaDialog.FileName))
                return;            

            _state.SchemaPath = schemaDialog.FileName;
            RefreshView();
            NotifyChange();
        }

        private void cbEngine_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbEngine.Text == null)
                return;

            _state.TemplatingEngine = cbEngine.Text;

            NotifyChange();
        }

        private void NotifyChange()
        {
            if (StateChanged != null)
            {
                StateChanged(this, new EventArgs());
            }
        }
        
        private void btnPreview_Click(object sender, EventArgs ea)
        {
            try
            {
                var url = SetupHelpers.GetReportingServiceUrl();
                var service = new ReportingService(url);

                var r = service.RenderPreviewAsync(new RenderRequest()
                {
                    Data = ReadSchema(),
                    Template = new Template()
                    {
                        Html = ReadHtml(),
                        Helpers = ReadHelpers(),
                        RenderingEngine = _state.TemplatingEngine
                    },
                    Options = new RenderOptions() { Async = false, Type = tbReceipe.Text  }
                }).Result;

                var reader = new StreamReader(r.Content);

                var str = reader.ReadToEnd();

                var tempFile = Path.GetTempFileName();
                tempFile = Path.ChangeExtension(tempFile, r.FileExtension);

                File.WriteAllText(tempFile, str);

                Process.Start(tempFile);
            }
            catch (Exception e)
            {
            }
        }

        private string ReadHtml()
        {
            var fileName = Path.GetFileName(_fileName);
            var dir = new FileInfo(_fileName).Directory.FullName;

            return File.ReadAllText(Path.Combine(dir, fileName + ".html"));
        }

        private string ReadHelpers()
        {
            var fileName = Path.GetFileName(_fileName);
            var dir = new FileInfo(_fileName).Directory.FullName;

            return File.ReadAllText(Path.Combine(dir, fileName + ".js"));
        }
        
        private object ReadSchema()
        {
            return JObject.Parse(File.ReadAllText(_state.SchemaPath));
        }        
    }

    public class ReportDefinition
    {
        public string Timeout { get; set; }
        public string SchemaPath { get; set; }
        public string TemplatingEngine { get; set; }
    }
}
