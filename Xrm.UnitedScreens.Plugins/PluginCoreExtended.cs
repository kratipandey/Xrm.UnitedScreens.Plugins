using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
//using NetEnt.Dev.Helper.XrmHelper;
//using Newtonsoft.Json;
using Xrm.US.Helper.XrmHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.US.Plugins
{
    public abstract class PluginCoreExtended<T> : PluginCore where T : Entity
    {
        public PluginCoreExtended(IServiceProvider svcProvider, string logicalName, PluginStage pluginStage, string unsecureConfig = null, string secureConfig = null) : base(svcProvider, logicalName, pluginStage, unsecureConfig, secureConfig)
        {
            
        }

        public T TargetEntity
        {
            get
            {
                return Target?.ToEntity<T>();
            }
        }

        public T PreImageEntity
        {
            get
            {
                return PreImage?.ToEntity<T>();
            }
        }

        public T PostImageEntity
        {
            get
            {
                return PostImage?.ToEntity<T>();
            }
        }

        

        abstract public void ExecutePlugin();

        public void Run()
        {
            try
            {
                Trace("Run");
                ExecutePlugin();
                Trace("Executed");
            }
            
            catch (InvalidPluginExecutionException ex)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                throw new InvalidPluginExecutionException(
                    String.Format("An error occurred in the {0} plug-in: {1}", this.GetType().ToString(), ex.ToString()), ex);
            }
        }

        
    }
}
