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

namespace JsReportVSTools
{
    public partial class JsRepSetup : UserControl
    {
        private ReportDefinition _state = new ReportDefinition();
        private string _fileName;

        public JsRepSetup()
        {
            InitializeComponent();
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
            tbTemplatingEngine.Text = _state.TemplatingEngine;

            if (string.IsNullOrEmpty(_state.SchemaPath)) {
                lblSchemaFilePath.Hide();
             }
            else {
                lblSchemaFilePath.Text = _state.SchemaPath;
                lblSchemaFilePath.Show();
            }         
        }
  
        private void tbTimeout_TextChanged(object sender, EventArgs e)
        {
           _state.Timeout = tbTimeout.Text;
           StateChanged(this, new EventArgs());
        }

        private void openSchemaDialog_click(object sender, EventArgs e)
        {
            schemaDialog.ShowDialog();

            if (string.IsNullOrEmpty(schemaDialog.FileName))
                return;            

            _state.SchemaPath = schemaDialog.FileName;
            RefreshView();
            StateChanged(this, new EventArgs());
        }

        private void tbTemplatingEngine_TextChanged(object sender, EventArgs e)
        {
            _state.TemplatingEngine = tbTemplatingEngine.Text;
            StateChanged(this, new EventArgs());
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            var service = new ReportingService("http://localhost:3000/");

            var r = service.RenderPreviewAsync(new RenderRequest()
            {
                Data = null,
                Template = new Template()
                {
                    RenderingEngine = "jsrender",
                    Html = "Hey",                    
                },
                Options = new RenderOptions() { Async = false }                
            }).Result;

            var reader = new StreamReader(r);

            var str = reader.ReadToEnd();
        }    
    }

    public class ReportDefinition
    {
        public string Timeout { get; set; }
        public string SchemaPath { get; set; }
        public string TemplatingEngine { get; set; }
    }
}
