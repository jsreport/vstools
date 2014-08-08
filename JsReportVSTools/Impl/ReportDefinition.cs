namespace JsReportVSTools.Impl
{
    public class ReportDefinition
    {
        public string SchemaPath { get; set; }
        public string Engine { get; set; }

        public string Recipe { get; set; }

        public PhantomDefinition Phantom { get; set; } 
    }
}