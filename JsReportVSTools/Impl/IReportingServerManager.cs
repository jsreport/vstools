using System.Collections.Generic;
using System.Threading.Tasks;
using EnvDTE;

namespace JsReportVSTools.Impl
{
    public interface IReportingServerManager
    {
        RemoteTask<int> StopAsync();
        RemoteTask<int> EnsureStartedAsync();

        RemoteTask<IEnumerable<string>> GetRecipesAsync();
        RemoteTask<IEnumerable<string>> GetEnginesAsync();
        RemoteTask<int> SynchronizeTemplatesAsync();

        object CreateReportingService();
        string ServerUri { get; }

        IEnumerable<string> SampleDataItems { get; } 
        RemoteTask<object> RenderAsync(string shortid, string sampleDataName);
    }
}