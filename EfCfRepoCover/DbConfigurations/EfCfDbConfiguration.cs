using System;
using System.Data.Entity;
using System.Runtime.Remoting.Messaging;
using Logging.Interfaces;

namespace EfCfRepoCoverLib.DbConfigurations
{
    public class EfCfDbConfiguration : DbConfiguration
    {
        public ILogging Logger { get; set; }

        public static bool IsExecutionStrategySuspended
        {
            get
            {
                var isExecutionStrategySuspended = (bool?)CallContext.LogicalGetData(Constants.LOGICAL_CALL_CONTEXT_OBJECT_NAME) ?? false;
                return isExecutionStrategySuspended;
            }
            set
            {
                CallContext.LogicalSetData(Constants.LOGICAL_CALL_CONTEXT_OBJECT_NAME, value);
            }
        }
    }
}
