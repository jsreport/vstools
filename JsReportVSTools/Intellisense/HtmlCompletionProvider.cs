//using JsReportVSTools.JsRepEditor;
//using Microsoft.Html.Editor.Intellisense;
//using Microsoft.VisualStudio.Utilities;
//using Microsoft.Web.Editor;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace JsReportVSTools.Intellisense
//{
//    [HtmlCompletionProvider(CompletionType.Values, "*", "*")]
//    [ContentType(HtmlContentTypeDefinition.HtmlContentType)]
//    [ContentType(JScriptContentTypeDefinition.JScriptContentType)]
//    [Export(typeof(IHtmlCompletionListProvider))]
//    [Name("HtmlCompletionProvider")]
//    public class HtmlCompletionProvider : IHtmlCompletionListProvider
//    {
//        public CompletionType CompletionType
//        {
//            get { return CompletionType.Values; }
//        }

//        public IList<HtmlCompletion> GetEntries(HtmlCompletionContext context)
//        {
//            var list = new List<HtmlCompletion>();
//            list.Add(new SimpleHtmlCompletion("Foo"));
//            return list;
//            return SetupHelpers.Foo().Select(s => new SimpleHtmlCompletion(s)).Cast<HtmlCompletion>().ToList();
//        }
//    }
//}
