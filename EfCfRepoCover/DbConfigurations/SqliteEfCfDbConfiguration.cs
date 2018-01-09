using System;
using System.Data.Entity.Infrastructure;
using EfCfRepoCoverLib.ConnectionResiliency;
using Logging.Interfaces;
using MySql.Data.Entity;

namespace EfCfRepoCoverLib.DbConfigurations
{
    public class SqliteEfCfDbConfiguration : EfCfDbConfiguration
    {
        public IDbExecutionStrategy DbExecutionStrategy { get; private set; }

        public SqliteEfCfDbConfiguration()
        {
            this.Logger = null;
            Initialize();
        }

        public SqliteEfCfDbConfiguration(ILogging logger)
        {
            this.Logger = logger;
            Initialize(logger);
        }

        public void Initialize(ILogging logger = null)
        {
            var providerInvariantName = Constants.PROVIDER_INVARIANTNAME_SQLITE; // (e.g. "System.Data.SQLite.EF6")

            IDbExecutionStrategy dbExecutionStrategy = null;

            dbExecutionStrategy = EfCfDbConfiguration.IsExecutionStrategySuspended ? (IDbExecutionStrategy)new DefaultExecutionStrategy() : new SqliteEfCfExecutionStrategy(logger);

            // If 'execution strategy' is suspended, use the 'DefaultExecutionStrategy'.
            //     'DefaultExecutionStrategy' allows user-initiated transactions as there is no automated 'retry' logic (such as the 'ShouldRetryOn()' overridden method in 'EfCfExecutionStrategy').
            //     'EfCfExecutionStrategy' inherits 'DbExecutionStrategy' and overrides the 'ShouldRetryOn()' method to allow automated 'retry' logic via EntityFramework.
            this.SetExecutionStrategy(providerInvariantName, 
                                      () => EfCfDbConfiguration.IsExecutionStrategySuspended ? (IDbExecutionStrategy)new DefaultExecutionStrategy() : new SqliteEfCfExecutionStrategy(logger));

            this.DbExecutionStrategy = dbExecutionStrategy;
        }
    }
}
