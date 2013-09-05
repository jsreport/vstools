using System;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using EnvDTE;
using System.IO;

namespace EditorMargin1
{
    /// <summary>
    /// A class detailing the margin's visual definition including both size and content.
    /// </summary>
    class EditorMargin1 : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "EditorMargin1";
        private IWpfTextView _textView;
        private bool _isDisposed = false;

        /// <summary>
        /// Creates a <see cref="EditorMargin1"/> for a given <see cref="IWpfTextView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        public EditorMargin1(IWpfTextView textView)
        {
            _textView = textView;

            this.Height = 40;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.LightBlue);

            // Add a green colored label that says "Hello World!"
            Button previewBtn = new Button();
            previewBtn.Click += previewBtn_Click;
            previewBtn.Content = "Preview";
            this.Children.Add(previewBtn);

        }

        void previewBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

            var currentPath = dte.ActiveDocument.Path;

            var helpersName = dte.ActiveDocument.FullName.Replace(".html", ".js");

            var helpersContent = File.ReadAllText(Path.Combine(currentPath, helpersName));

            using (var edit = _textView.TextBuffer.CreateEdit())
            {
                edit.Delete(0, edit.Snapshot.GetText().Length);
                edit.Insert(0, helpersContent);
                edit.Apply();
            }           
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(MarginName);
        }

        #region IWpfTextViewMargin Members

        /// <summary>
        /// The <see cref="Sytem.Windows.FrameworkElement"/> that implements the visual representation
        /// of the margin.
        /// </summary>
        public System.Windows.FrameworkElement VisualElement
        {
            // Since this margin implements Canvas, this is the object which renders
            // the margin.
            get
            {
                ThrowIfDisposed();
                return this;
            }
        }

        #endregion

        #region ITextViewMargin Members

        public double MarginSize
        {
            // Since this is a horizontal margin, its width will be bound to the width of the text view.
            // Therefore, its size is its height.
            get
            {
                ThrowIfDisposed();
                return this.ActualHeight;
            }
        }

        public bool Enabled
        {
            // The margin should always be enabled
            get
            {
                ThrowIfDisposed();
                return true;
            }
        }

        /// <summary>
        /// Returns an instance of the margin if this is the margin that has been requested.
        /// </summary>
        /// <param name="marginName">The name of the margin requested</param>
        /// <returns>An instance of EditorMargin1 or null</returns>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return (marginName == EditorMargin1.MarginName) ? (IWpfTextViewMargin)this : null;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
        #endregion
    }
}
