using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using JsReportVSTools.Impl;

namespace JsReportVSTools
{
    public partial class JsRepSetup : UserControl
    {
        private ReportDefinition _state = new ReportDefinition();

        public JsRepSetup()
        {
            InitializeComponent();

            cbEngine.ValueMember = "Value";
            cbEngine.DisplayMember = "Text";

            cbRecipe.ValueMember = "Value";
            cbRecipe.DisplayMember = "Text";

            cbSchema.ValueMember = "Value";
            cbSchema.DisplayMember = "Text";
        }

       
        public bool ReadOnly { get; set; }

        public async void LoadStateFromFile(string fileName)
        {
            _state = SetupHelpers.ReadReportDefinition(fileName);
            
            try
            {
                await FillEngines(fileName);
                await FillRecipes(fileName);

                SetupHelpers.GetSchemas().ToList().ForEach(s => cbSchema.Items.Add(new { Text = s, Value = s }));

                RefreshView();
            }
            catch (MissingJsReportEmbeddedDllException e)
            {
                MessageBox.Show(e.Message, "jsreport error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Task.Delay(1000).ContinueWith((t) => Task.Run(() => SetupHelpers.TryCloseActiveDocument()));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error");
                Task.Delay(1000).ContinueWith((t) => Task.Run(() => SetupHelpers.TryCloseActiveDocument()));
            }
        }

        public void SaveStateToFile(string fileName)
        {
            var sb = new StringBuilder();
            var ms = new StringWriter(sb);

            var s = new XmlSerializer(typeof (ReportDefinition));
            s.Serialize(ms, _state);

            File.WriteAllText(fileName, sb.ToString());
        }

        public event EventHandler StateChanged;

        private void RefreshView()
        {
            cbEngine.Text = _state.Engine;
            cbRecipe.Text = _state.Recipe;
            cbSchema.Text = _state.Schema;

            lnkServerLocation.Text = SetupHelpers.EmbeddedServerManager.CurrentShadowBinFolder;
            lnkServerUrl.Text = SetupHelpers.EmbeddedServerManager.EmbeddedServerUri;

            pnlPhantom.Visible = _state.Recipe == "phantom-pdf";

            _state.Phantom = _state.Phantom ?? new PhantomDefinition();

            tbFooterHeight.Text = _state.Phantom.FooterHeight;
            tbHeaderHeight.Text = _state.Phantom.HeaderHeight;
            tbMargin.Text = _state.Phantom.Margin;
            tbPaperHeight.Text = _state.Phantom.Height;
            tbPaperWidth.Text = _state.Phantom.Width;
            rbFooter.Text = _state.Phantom.Footer;
            rbHeader.Text = _state.Phantom.Header;
            cbFormat.Text = _state.Phantom.Format;
            cbPaperOrientation.Text = _state.Phantom.Orientation;
        }

        private void cbEngine_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbEngine.Text == null)
                return;

            _state.Engine = cbEngine.Text;

            NotifyChange();
        }

        private void cbRecipe_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbRecipe.Text == null)
                return;

            _state.Recipe = cbRecipe.Text;

            pnlPhantom.Visible = cbRecipe.Text == "phantom-pdf";

            NotifyChange();
        }

        private void NotifyChange()
        {
            if (StateChanged != null)
            {
                StateChanged(this, new EventArgs());
            }
        }

        private async Task FillRecipes(string fileName)
        {
            foreach (string r in await SetupHelpers.GetRecipesAsync(fileName).ConfigureAwait(false))
            {
                cbRecipe.Items.Add(new {Text = r, Value = r});
            }
        }

        private async Task FillEngines(string fileName)
        {
            foreach (string r in await SetupHelpers.GetEngines(fileName).ConfigureAwait(false))
            {
                cbEngine.Items.Add(new {Text = r, Value = r});
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

        private void tbMargin_TextChanged(object sender, EventArgs e)
        {
            _state.Phantom.Margin = tbMargin.Text;
            NotifyChange();
        }

        private void tbPaperWidth_TextChanged(object sender, EventArgs e)
        {
            _state.Phantom.Width = tbPaperWidth.Text;
            NotifyChange();
        }

        private void cbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbFormat.Text == null)
                return;

            _state.Phantom.Format = cbFormat.Text;

            NotifyChange();
        }

        private void tbPaperHeight_TextChanged(object sender, EventArgs e)
        {
            _state.Phantom.Height = tbPaperHeight.Text;
            NotifyChange();
        }

        private void cbPaperOrientation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbPaperOrientation.Text == null)
                return;

            _state.Phantom.Orientation = cbPaperOrientation.Text;

            NotifyChange();
        }

        private void tbFooterHeight_TextChanged(object sender, EventArgs e)
        {
            _state.Phantom.FooterHeight = tbFooterHeight.Text;
            NotifyChange();
        }

        private void tbHeaderHeight_TextChanged(object sender, EventArgs e)
        {
            _state.Phantom.HeaderHeight = tbHeaderHeight.Text;
            NotifyChange();
        }

        private void rbHeader_TextChanged(object sender, EventArgs e)
        {
            _state.Phantom.Header = rbHeader.Text;
            NotifyChange();
        }

        private void rbFooter_TextChanged(object sender, EventArgs e)
        {
            _state.Phantom.Footer = rbFooter.Text;
            NotifyChange();
        }

        private void cbSchema_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbSchema.Text == null)
                return;

            _state.Schema = cbSchema.Text;

            NotifyChange();
        }
    }
}