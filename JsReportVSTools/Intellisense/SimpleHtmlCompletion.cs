﻿using Microsoft.Html.Editor.Intellisense;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.Web.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace JsReportVSTools.Intellisense
{
    public class SimpleHtmlCompletion : HtmlCompletion
    {
        private static ImageSource _glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);

        public SimpleHtmlCompletion(string displayText)
            : base(displayText, displayText, null, _glyph, HtmlIconAutomationText.AttributeIconText)
        { }

        public SimpleHtmlCompletion(string displayText, string description)
            : base(displayText, displayText, description, _glyph, HtmlIconAutomationText.AttributeIconText)
        { }
    }
}
