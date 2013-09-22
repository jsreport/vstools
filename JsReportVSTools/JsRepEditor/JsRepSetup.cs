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
using System.Threading.Tasks;

namespace JsReportVSTools
{
    public partial class JsRepSetup : UserControl
    {
        private ReportDefinition _state = new ReportDefinition();
        private string _fileName;

        public JsRepSetup()
        {
            InitializeComponent();

            cbEngine.ValueMember = "Value";
            cbEngine.DisplayMember = "Text";

            foreach (string r in GetEnginesAsync().GetAwaiter().GetResult())
            {
                cbEngine.Items.Add(new { Text = r, Value = r });
            }

            cbRecipe.ValueMember = "Value";
            cbRecipe.DisplayMember = "Text";
           
            foreach (string r in GetRecipesAsync().GetAwaiter().GetResult())
            {
                cbRecipe.Items.Add(new { Text = r, Value = r });
            }          
        }            

        public void LoadFile(string fileName)
        {
            _fileName = fileName;

            try
            {
                var s = new XmlSerializer(typeof(ReportDefinition));
                _state = s.Deserialize(new StreamReader(fileName)) as ReportDefinition;
            }
            catch (Exception e)
            {
            }
            RefreshView();         
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
            cbEngine.Text = _state.Engine;

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

            _state.SchemaPath = SetupHelpers.GetSolutionFileFullName(schemaDialog.FileName);
            RefreshView();
            NotifyChange();
        }

        private void cbEngine_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbEngine.Text == null)
                return;

            _state.Engine = cbEngine.Text;

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
           var url = SetupHelpers.GetReportingServiceUrl();
           var service = new ReportingService(url);

           var r = service.RenderPreviewAsync(new RenderRequest()
           {
               Data = ReadSchema(),
               Template = new Template()
               {
                   Html = ReadHtml(),
                   Helpers = ReadHelpers(),
                   Engine = _state.Engine
               },
               Options = new RenderOptions() { Async = false, Recipe = cbRecipe.Text}
           }).Result;

           var tempFile = Path.GetTempFileName();
           tempFile = Path.ChangeExtension(tempFile, r.FileExtension);

           using (var fileStream = File.Create(tempFile))
           {
               r.Content.CopyTo(fileStream);
           }               

           Process.Start(tempFile);
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
            return JObject.Parse(SetupHelpers.ReadFile(_state.SchemaPath));
        }

        private void btnHelpers_Click(object sender, EventArgs e)
        {
            SetupHelpers.OpenHelpers();
        }

        private void btnHtml_Click(object sender, EventArgs e)
        {
            SetupHelpers.OpenHtml();
        }

        private async Task<IEnumerable<string>> GetRecipesAsync()
        {
            var url = SetupHelpers.GetReportingServiceUrl();
            var service = new ReportingService(url);

            return await service.GetRecipesAsync().ConfigureAwait(false);
        }

        private async Task<IEnumerable<string>> GetEnginesAsync()
        {
            var url = SetupHelpers.GetReportingServiceUrl();
            var service = new ReportingService(url);

            return await service.GetEnginesAsync().ConfigureAwait(false);
        }
    }

    public class ReportDefinition
    {        
        public string Timeout { get; set; }
        public string SchemaPath { get; set; }
        public string Engine { get; set; }
    }
}
