using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace JsReportVSTools.Impl
{
    public abstract class ReportingServerManagerBase : MarshalByRefObject
    {
        private readonly dynamic _configuration;

        protected ReportingServerManagerBase(object configuration)
        {
            _configuration = configuration;
        }

        public abstract object CreateReportingService();

        public RemoteTask<IEnumerable<string>> GetRecipesAsync()
        {
            dynamic rs = CreateReportingService();

            return RemoteTask.ServerStart<IEnumerable<string>>(cts => rs.GetRecipesAsync());
        }

        public RemoteTask<IEnumerable<string>> GetEnginesAsync()
        {
            dynamic rs = CreateReportingService();

            return RemoteTask.ServerStart<IEnumerable<string>>(cts => rs.GetEnginesAsync());
        }

        public RemoteTask<int> SynchronizeTemplatesAsync()
        {
            dynamic rs = CreateReportingService();

            return RemoteTask.ServerStart<int>(async cts =>
            {
                if (_configuration != null)
                {
                    foreach (KeyValuePair<string, object> schema in _configuration.Schemas)
                    {
                        await rs.CreateOrUpdateSchema(schema.Key, JsonConvert.SerializeObject(schema.Value));
                    }
                }

                await rs.SynchronizeTemplatesAsync();
                return 0;
            });
        }

        public RemoteTask<object> RenderAsync(string shortid, string schemaName)
        {
            dynamic rs = CreateReportingService();

            return RemoteTask.ServerStart<object>(async cts =>
            {
                dynamic report = await rs.RenderAsync(shortid, PrepareSchema(schemaName));
                return new Report
                {
                    Content = report.Content,
                    FileExtension = report.FileExtension
                };
            });
        }

        public IEnumerable<string> Schemas
        {
            get
            {
                if (_configuration == null)
                    return Enumerable.Empty<string>();

                IEnumerable<string> staticSchemas = _configuration.Schemas.Keys;
                IEnumerable<string> dynamicSchemas = _configuration.DynamicSchemas.Keys;

                return staticSchemas.Concat(dynamicSchemas).ToList();
            }
        }

        private object PrepareSchema(string schemaName)
        {
            if (_configuration == null || string.IsNullOrEmpty(schemaName))
                return null;

            if (!_configuration.DynamicSchemas.ContainsKey(schemaName))
                return null;

            return _configuration.DynamicSchemas[schemaName]();
        }

        //the MarshalByRefObject has some strange expiracy time so we need to say that these objects should stay forever active
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    [Serializable]
    public class Report : MarshalByRefObject
    {
        public Stream Content { get; set; }
        public string FileExtension { get; set; }
    }
}