using System.Threading.Tasks;

namespace JsReportVSTools.Impl
{
    public interface IReportingServerFactory
    {
        IReportingServerManager Create(string binFolder, string mainAssemblyName);
    }
}