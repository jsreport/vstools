using System.Threading.Tasks;
using EnvDTE;

namespace JsReportVSTools.Impl
{
    public interface IReportingServerManager
    {
        Task StopAsync();
        Task EnsureStartedAsync(string fileName = null);
        object CreateReportingService();
        string ServerUri { get; }
    }
}