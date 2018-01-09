using System;
using System.Data.Entity.Infrastructure;
using EfCfRepoCoverLib.ConnectionResiliency;
using Logging.Interfaces;

namespace EfCfRepoCoverLib.DbConfigurations
{
    public class MsSqlServerEfCfDbConfiguration : EfCfDbConfiguration
    {
        public IDbExecutionStrategy DbExecutionStrategy { get; private set; }

        public MsSqlServerEfCfDbConfiguration()
        {
            this.Logger = null;
            Initialize();
        }

        public MsSqlServerEfCfDbConfiguration(ILogging logger)
        {
            this.Logger = logger;
            Initialize(logger);
        }

        public void Initialize(ILogging logger = null)
        {
            var providerInvariantName = Constants.PROVIDER_INVARIANTNAME_MSSQLSERVER; // (e.g. "System.Data.SqlClient")

            IDbExecutionStrategy dbExecutionStrategy = null;

            dbExecutionStrategy = EfCfDbConfiguration.IsExecutionStrategySuspended ? (IDbExecutionStrategy)new DefaultExecutionStrategy() : new MsSqlServerEfCfExecutionStrategy(logger);

            // If 'execution strategy' is suspended, use the 'DefaultExecutionStrategy'.
            //     'DefaultExecutionStrategy' allows user-initiated transactions as there is no automated 'retry' logic (such as the 'ShouldRetryOn()' overridden method in 'EfCfExecutionStrategy').
            //     'EfCfExecutionStrategy' inherits 'DbExecutionStrategy' and overrides the 'ShouldRetryOn()' method to allow automated 'retry' logic via EntityFramework.
            this.SetExecutionStrategy(providerInvariantName, 
                                      () => EfCfDbConfiguration.IsExecutionStrategySuspended ? (IDbExecutionStrategy)new DefaultExecutionStrategy() : new MsSqlServerEfCfExecutionStrategy(logger));

            this.DbExecutionStrategy = dbExecutionStrategy;
        }
    }
}
