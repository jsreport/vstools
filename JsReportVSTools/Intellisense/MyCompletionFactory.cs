//using System;
//using System.Collections.Generic;
//using System.Windows.Media;
//using Microsoft.Html.Editor.Intellisense;
//using Microsoft.VisualStudio.Language.Intellisense;
//using Microsoft.VisualStudio.OLE.Interop;
//using Microsoft.VisualStudio.Text;
//using Microsoft.Web.Editor;

//namespace JsReportVSTools.Intellisense
//{
//    public class MyCompletionFactory : CompletionSet
//    {
//        private static ImageSource _glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);



//        internal MyCompletionFactory() : base("JsReportVsTools", "JsReportVsTools", null, null, null) { }

//        private IList<Completion> _completions; 
//        public override IList<Completion> Completions
//        {
//            get
//            {
//                try
//                {
//                    if (_completions == null)
//                        _completions = new List<Completion>()
//                        {
//                            new Completion("sobota", "sobota", "description", _glyph, null),
//                            new Completion("nedele", "nedele", "description", _glyph, null)
//                        };
//                    return _completions;
//                }
//                catch
//                {
//                    return new List<Completion>();
//                }
//            }
//        }

//        //public override void SelectBestMatch()
//        //{
//        //    var completions = Completions;
//        //    SelectionStatus = new CompletionSelectionStatus(completions[0], true, true);
//        //}


//        public static CompletionSet Create(ITrackingSpan trackingSpan)
//        {
//            return  new MyCompletionFactory() {ApplicableTo = trackingSpan};
//        }

    
//    }
//}