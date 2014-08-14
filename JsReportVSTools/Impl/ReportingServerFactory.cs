using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace JsReportVSTools.Impl
{
    /// <summary>
    /// Creates proxy to a remote or embedded reporting manager. 
    /// </summary>
    public class ReportingServerFactory : MarshalByRefObject, IReportingServerFactory
    {
        //here we are in the separate app domain and use reflection to work with jsreport.Client sdk to asure independency on dll version

        public IReportingServerManager Create(string binFolder, string mainAssemlyName)
        {
            //first try to load ReportingStartup.cs file from current project

            Type startupType = AppDomain.CurrentDomain.Load(mainAssemlyName).GetTypes().FirstOrDefault(t => t.Name.Contains("ReportingStartup"));

            if (startupType == null)
                return CreateEmbeddedServerManager(binFolder, null);

            dynamic startup = Activator.CreateInstance(startupType);
            
            Type configurationType = AppDomain.CurrentDomain.Load("jsreport.Client").GetType("jsreport.Client.VSConfiguration.VSReportingConfiguration");

            dynamic configuration = Activator.CreateInstance(configurationType);
            startup.Configure(configuration);

            if (string.IsNullOrEmpty(configuration.RemoteServerUri))
                return CreateEmbeddedServerManager(binFolder, configuration);

            return CreateRemoteServerManager(configuration.RemoteServerUri, configuration.RemoteServerUsername, configuration.RemoteServerPassword, configuration);
        }

        private IReportingServerManager CreateEmbeddedServerManager(string binFolder, object configuration)
        {
            return new EmbeddedServerManager(binFolder, configuration);
        }

        private IReportingServerManager CreateRemoteServerManager(string remoteUri, string username, string password, object configuration)
        {
            return new RemoteServerManager(remoteUri, username, password, configuration);
        }

        //the MarshalByRefObject has some strange expiracy time so we need to say that these objects should stay forever active
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}