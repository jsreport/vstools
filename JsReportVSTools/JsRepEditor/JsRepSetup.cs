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
using System.Threading;
using System.Drawing;

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
                       
            cbRecipe.ValueMember = "Value";
            cbRecipe.DisplayMember = "Text";
                       
            pbPreview.Image = Resources.preview;            

            if (string.IsNullOrEmpty(SetupHelpers.GetReportingServiceUrl()))
            {
                var dialog = new FirstTimeConfigDialog();
                dialog.FormClosed += JsRepSetup_Load;
                dialog.ShowDialog(this);               
            }
        }            

        public void LoadStateFromFile(string fileName)
        {
            _fileName = fileName;

            _state = SetupHelpers.ReadReportDefinition(fileName);
            RefreshView();         
        }

        public void SaveStateToFile(string fileName)
        {            
            var sb = new StringBuilder();
            var ms = new StringWriter(sb);

            var s = new XmlSerializer(typeof(ReportDefinition));            
            s.Serialize(ms, _state);

            File.WriteAllText(fileName, sb.ToString());
        }

        public bool ReadOnly { get; set; }

        public event EventHandler StateChanged;

        private async void JsRepSetup_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SetupHelpers.GetReportingServiceUrl()))
            {
                await FillEngines();
                await FillRecipes();
            }
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
        
        private async void btnPreview_Click(object sender, EventArgs ea)
        {
            await DoPreview();
        }

        private async Task DoPreview()
        {
            try
            {
                var report = await SetupHelpers.RenderPreviewAsync(ReadSchema(), ReadHtml(), ReadHelpers(), _state.Engine, cbRecipe.Text);
                var tempFile = Path.GetTempFileName();
                tempFile = Path.ChangeExtension(tempFile, report.FileExtension);

                using (var fileStream = File.Create(tempFile))
                {
                    report.Content.CopyTo(fileStream);
                }

                Process.Start(tempFile);
            }
            catch (JsReportException e)
            {
                MessageBox.Show(this, e.ResponseErrorMessage, "Error when processing template", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }  
            catch (Exception e)
            {
                MessageBox.Show(this, e.ToString(), "Error when processing template", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            return JObject.Parse(SetupHelpers.ReadFile(_state.SchemaPath));
        }        
        
        private async Task FillRecipes()
        {
            foreach (string r in await SetupHelpers.GetRecipesAsync())
            {
                cbRecipe.Items.Add(new { Text = r, Value = r });
            }
        }

        private async Task FillEngines()
        { 
            foreach (string r in await SetupHelpers.GetEngines())
            {
                cbEngine.Items.Add(new { Text = r, Value = r });
            }
        }      

        private void lnkHelpers_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SetupHelpers.OpenHelpers();
        }

        private void lnkHtml_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SetupHelpers.OpenHtml();
        }

        private async void pnlPreview_Click(object sender, EventArgs e)
        {
            await DoPreview();
        }

        private async void pbPreview_Click(object sender, EventArgs e)
        {
            await DoPreview();
        }

        private async void lblPreview_Click(object sender, EventArgs e)
        {
            await DoPreview();
        } 
           
        private void pnlPreview_MouseEnter(object sender, EventArgs e)
        {
            pnlPreview.BackColor = Color.LightBlue;
        }

        private void pnlPreview_MouseLeave(object sender, EventArgs e)
        {           
            if (pnlPreview.Bounds.Contains(Cursor.Position))
            {
                pnlPreview.BackColor = Color.LightBlue;
            }
            else {
                pnlPreview.BackColor = Color.Transparent;
            }
        }  
       
    }

    public class ReportDefinition
    {        
        public string Timeout { get; set; }
        public string SchemaPath { get; set; }
        public string Engine { get; set; }
    }
}
