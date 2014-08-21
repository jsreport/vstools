using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using EnvDTE;
using JsReportVSTools.Impl;

namespace JsReportVSTools.JsRepEditor
{
    /// <summary>
    ///     Interaction logic for JsReportEditor.xaml
    /// </summary>
    public partial class JsReportEditor : UserControl
    {
        private ReportDefinition _state;
        private string _fileName;
        private bool _reloadInProgress;
        private readonly object _locker = new object();
        private bool _refreshRequired;
        private bool _doRefreshWhenLayoutWillChange;

        public JsReportEditor()
        {
            InitializeComponent();

            LayoutUpdated += JsReportEditor_LayoutUpdated;
            IsVisibleChanged += JsReportEditor_IsVisibleChanged;
        }
        
        void JsReportEditor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //somehow it frozes when I do ReloadOptions in this method, so I postpone it until next LayoutUpdated event

            if (_refreshRequired && Visibility == Visibility.Visible)
                _doRefreshWhenLayoutWillChange = true;
        }

        async void JsReportEditor_LayoutUpdated(object sender, EventArgs e)
        {
            if (_doRefreshWhenLayoutWillChange)
            {
                _doRefreshWhenLayoutWillChange = false;
                await ReloadOptions();
            }
        }

        public async Task ReloadOptions(bool throwErrors = true)
        {
            lock (_locker)
            {
                if (_reloadInProgress)
                    return;

                _reloadInProgress = true;
            }

            try
            {
                Trace.WriteLine("Starting");
                Mouse.OverrideCursor = Cursors.Wait;

                await FillEngines(_fileName);
                await FillRecipes(_fileName);

                CbSampleData.Items.Clear();
                (await SetupHelpers.GetSampleData()).ToList()
                    .ForEach(s => CbSampleData.Items.Add(new CbItem() { Text = s, Id = s }));

                RefreshView();
            }
            catch (WeakJsReportException e)
            {
                Trace.TraceError("Error when reloading editor drop downs " + e.ToString());
                if (throwErrors)
                {
                    MessageBox.Show(e.Message, "jsreport error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Task.Delay(1000).ContinueWith(t => Task.Run(() => SetupHelpers.TryCloseActiveDocument()));
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError("Error when reloading editor drop downs " + exception.ToString());
                if (throwErrors)
                {
                    MessageBox.Show(exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Task.Delay(1000).ContinueWith(t => Task.Run(() => SetupHelpers.TryCloseActiveDocument()));
                }
            }
            finally
            {
                Trace.WriteLine("Finishing");
                Mouse.OverrideCursor = null;
                _reloadInProgress = false;
                _refreshRequired = false;
            }
        }

        public async Task LoadStateFromFile(string fileName)
        {
            _fileName = fileName;

            _state = SetupHelpers.ReadReportDefinition(fileName);

            await ReloadOptions();
        }

        private void RefreshView()
        {
            CbEngine.Text = _state.Engine;
            CbRecipe.Text = _state.Recipe;
            CbSampleData.Text = _state.SampleData;

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
            Dispatcher.Invoke(() => CbRecipe.Items.Clear());

            foreach (string r in await SetupHelpers.GetRecipesAsync(fileName).ConfigureAwait(false))
            {
                Trace.TraceInformation("adding " + r);
                Dispatcher.Invoke(() => { CbRecipe.Items.Add(new CbItem() {Text = r, Id = r}); });
            }
        }

        private async Task FillEngines(string fileName)
        {
            Dispatcher.Invoke(() => CbEngine.Items.Clear());
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

        private void CbSampleData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
                return;

            _state.SampleData = ((CbItem) e.AddedItems[0]).Text;

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

        private void BtnSampleData_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_state.SampleData))
                return;

            SetupHelpers.OpenSampleData(_state);
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                SetupHelpers.OpenJsReportInBrowser();
            }
        }

        public void MarkRerfeshRequired()
        {
            _refreshRequired = true;
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