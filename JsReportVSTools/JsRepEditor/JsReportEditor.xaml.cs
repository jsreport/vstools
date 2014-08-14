using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using JsReportVSTools.Impl;

namespace JsReportVSTools.JsRepEditor
{
    /// <summary>
    ///     Interaction logic for JsReportEditor.xaml
    /// </summary>
    public partial class JsReportEditor : UserControl
    {
        private ReportDefinition _state;

        public JsReportEditor()
        {
            InitializeComponent();
        }

        public async void LoadStateFromFile(string fileName)
        {
            _state = SetupHelpers.ReadReportDefinition(fileName);

            try
            {
                await FillEngines(fileName);
                await FillRecipes(fileName);

                SetupHelpers.GetSchemas()
                    .ToList()
                    .ForEach(s =>
                    {
                        var item = new CbItem() {Text = s, Id = s};
                        if (!CbSchema.Items.Contains(item))
                            CbSchema.Items.Add(item);
                    });

                RefreshView();
            }
            catch (MissingJsReportDllException e)
            {
                MessageBox.Show(Window.GetWindow(this), e.Message, "jsreport error", MessageBoxButton.OK, MessageBoxImage.Error);
                Task.Delay(1000).ContinueWith(t => Task.Run(() => SetupHelpers.TryCloseActiveDocument()));
            }
            catch (Exception exception)
            {
                MessageBox.Show(Window.GetWindow(this), exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Task.Delay(1000).ContinueWith(t => Task.Run(() => SetupHelpers.TryCloseActiveDocument()));
            }
        }

        private void RefreshView()
        {
            CbEngine.Text = _state.Engine;
            CbRecipe.Text = _state.Recipe;
            CbSchema.Text = _state.Schema;

            PnlPhantom.Visibility = _state.Recipe == "phantom-pdf" ? Visibility.Visible : Visibility.Hidden;

            _state.Phantom = _state.Phantom ?? new PhantomDefinition();

            TbFooterHeight.Text = _state.Phantom.FooterHeight;
            TbHeaderHeight.Text = _state.Phantom.HeaderHeight;
            TbMargin.Text = _state.Phantom.Margin;
            TbPaperHeight.Text = _state.Phantom.Height;
            TbPaperWidth.Text = _state.Phantom.Width;
            TbFooter.Text = _state.Phantom.Footer;
            TbHeader.Text = _state.Phantom.Header;
            CbPaperFormat.Text = _state.Phantom.Format;
            CbPaperOrientation.Text = _state.Phantom.Orientation;
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

        private async Task FillRecipes(string fileName)
        {
            foreach (string r in await SetupHelpers.GetRecipesAsync(fileName).ConfigureAwait(false))
            {
                Dispatcher.Invoke(() => { CbRecipe.Items.Add(new CbItem() {Text = r, Id = r}); });
            }
        }

        private async Task FillEngines(string fileName)
        {
            foreach (string r in await SetupHelpers.GetEnginesAsync(fileName).ConfigureAwait(false))
            {
                Dispatcher.Invoke(() => { CbEngine.Items.Add(new CbItem() {Text = r, Id = r}); });
            }
        }

        private void NotifyChange()
        {
            if (StateChanged != null)
            {
                StateChanged(this, new EventArgs());
            }
        }

        private void CbEngine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
                return;

            _state.Engine = ((CbItem)e.AddedItems[0]).Text;

            NotifyChange();
        }

        private void CbSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
                return;

            _state.Schema = ((CbItem) e.AddedItems[0]).Text;

            NotifyChange();
        }

        private void CbRecipe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
                return;

            _state.Recipe = ((CbItem) e.AddedItems[0]).Text;

            PnlPhantom.Visibility = _state.Recipe == "phantom-pdf" ? Visibility.Visible : Visibility.Hidden;

            NotifyChange();
        }

        private void TbMargin_TextChanged(object sender, TextChangedEventArgs e)
        {
            _state.Phantom.Margin = TbMargin.Text;
            NotifyChange();
        }

        private void CbPaperFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
                return;

            _state.Phantom.Format = ((ComboBoxItem)e.AddedItems[0]).Content as string;

            NotifyChange();
        }

        private void CbPaperOrientation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
                return;

            _state.Phantom.Orientation = ((ComboBoxItem)e.AddedItems[0]).Content as string;

            NotifyChange();
        }

        private void TbPaperWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            _state.Phantom.Width = TbPaperWidth.Text;
            NotifyChange();

        }

        private void TbPaperHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            _state.Phantom.Height = TbPaperHeight.Text;
            NotifyChange();
        }

        private void TbHeaderHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            _state.Phantom.HeaderHeight = TbHeaderHeight.Text;
            NotifyChange();
        }

        private void TbFooterHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            _state.Phantom.FooterHeight = TbFooterHeight.Text;
            NotifyChange();
        }

        private void TbHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            _state.Phantom.Header = TbHeader.Text;
            NotifyChange();
        }

        private void TbFooter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _state.Phantom.Footer = TbFooter.Text;
            NotifyChange();
        }

        private async void BtnPreview_Click(object sender, RoutedEventArgs e)
        {
            await SetupHelpers.DoPreviewActiveItem();
        }

        private void BtnContent_Click(object sender, RoutedEventArgs e)
        {
            SetupHelpers.OpenHtml();
        }

        private void BtnHelpers_Click(object sender, RoutedEventArgs e)
        {
            SetupHelpers.OpenHelpers();
        }

        private void BtnSchema_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_state.Schema))
                return;

            SetupHelpers.OpenSchema(_state);
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                SetupHelpers.OpenEmbeddedServer();
            }
        }
    }

    public class CbItem
    {
        protected bool Equals(CbItem other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CbItem) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public string Id { get; set; }
        public string Text { get; set; }
    }
}