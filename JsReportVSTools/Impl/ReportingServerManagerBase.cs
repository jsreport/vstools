﻿using System;
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
                    foreach (KeyValuePair<string, object> sampleData in _configuration.SampleData)
                    {
                        await rs.CreateOrUpdateSampleData(sampleData.Key, JsonConvert.SerializeObject(sampleData.Value));
                    }
                }

                await rs.SynchronizeTemplatesAsync();
                return 0;
            });
        }

        public RemoteTask<object> RenderAsync(string shortid, string sampleDataName)
        {
            dynamic rs = CreateReportingService();

            return RemoteTask.ServerStart<object>(async cts =>
            {
                dynamic report = await rs.RenderAsync(shortid, PrepareSampleData(sampleDataName));
                return new Report
                {
                    Content = report.Content,
                    FileExtension = report.FileExtension
                };
            });
        }

        public IEnumerable<string> SampleDataItems
        {
            get
            {
                if (_configuration == null)
                    return Enumerable.Empty<string>();

                IEnumerable<string> staticSampleDataItems = _configuration.SampleData.Keys;
                IEnumerable<string> dynamicSampleDataItems = _configuration.DynamicSampleData.Keys;

                return staticSampleDataItems.Concat(dynamicSampleDataItems).ToList();
            }
        }

        private object PrepareSampleData(string sampleDataName)
        {
            if (_configuration == null || string.IsNullOrEmpty(sampleDataName))
                return null;

            if (!_configuration.DynamicSampleData.ContainsKey(sampleDataName))
                return null;

            return _configuration.DynamicSampleData[sampleDataName]();
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