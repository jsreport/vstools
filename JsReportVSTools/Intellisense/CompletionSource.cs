//using Microsoft.VisualStudio.Language.Intellisense;
//using Microsoft.VisualStudio.Text;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace JsReportVSTools.Intellisense
//{
//    public class CompletionSource : ICompletionSource, IDisposable
//    {
//        private readonly ITextBuffer _textBuffer;
//        private readonly CompletionSourceProvider _provider;
//        private ITrackingSpan _trackingSpan;
//        private bool _isDisposed;

//        public CompletionSource(CompletionSourceProvider provider, ITextBuffer textBuffer)
//        {
//            _textBuffer = textBuffer;
//            _provider = provider;
//        }

//        public void Dispose()
//        {
//            if (_isDisposed) return;
//            GC.SuppressFinalize(this);
//            _isDisposed = true;
//        }

   

//        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
//        {
//            var triggerPoint = session.GetTriggerPoint(_textBuffer).GetPoint(_textBuffer.CurrentSnapshot);
            
//            if (!session.Properties.TryGetProperty(typeof(ITrackingSpan), out _trackingSpan))
//                _trackingSpan = triggerPoint.Snapshot.CreateTrackingSpan(new Span(triggerPoint, 0), SpanTrackingMode.EdgeInclusive);

//            //CompletionSet sparkCompletions = CompletionSetFactory.GetCompletionSetFor(triggerPoint, _trackingSpan, _viewExplorer);

//            //var triggerPoint = session.GetTriggerPoint(textBuffer);
            
//            //var provider = textBuffer.CurrentSnapshot.GetCompletions(span, triggerPoint, options);

//            //ITrackingSpan applicableTo = _textBuffer.CurrentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);
            
//            //var set = new CompletionSet();
//            //set.Completions.Add(new SimpleHtmlCompletion("test"));
//            completionSets.Add(MyCompletionFactory.Create(_trackingSpan));

//            session.Committed += session_Committed;
//        }

//        private bool IsCompletionChar(ICompletionSession session, char completionChar)
//        {
//            var point = session.TextView.Caret.Position.BufferPosition;
//            return point.Position > 1 && (point - 1).GetChar() == completionChar;
//        }

//        void session_Committed(object sender, EventArgs e)
//        {
//            var session = sender as ICompletionSession;
//            if (session == null || session.IsDismissed) return;
//            if (!IsCompletionChar(session, '"')
//                && !IsCompletionChar(session, '\'')) return;

//            session.TextView.Caret.MoveToPreviousCaretPosition();
//        }
//    }
//}
