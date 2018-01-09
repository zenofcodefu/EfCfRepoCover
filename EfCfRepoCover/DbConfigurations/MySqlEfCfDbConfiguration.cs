using System;
using System.Data.Entity.Infrastructure;
using EfCfRepoCoverLib.ConnectionResiliency;
using Logging.Interfaces;
using MySql.Data.Entity;

namespace EfCfRepoCoverLib.DbConfigurations
{
    public class MySqlEfCfDbConfiguration : EfCfDbConfiguration
    {
        public IDbExecutionStrategy DbExecutionStrategy { get; private set; }

        public MySqlEfCfDbConfiguration()
        {
            this.Logger = null;
            Initialize();
        }

        public MySqlEfCfDbConfiguration(ILogging logger)
        {
            this.Logger = logger;
            Initialize(logger);
        }

        public void Initialize(ILogging logger = null)
        {
            //var providerInvariantName = Constants.PROVIDER_INVARIANTNAME_MYSQL; // (e.g. "MySql.Data.MySqlClient")

            IDbExecutionStrategy dbExecutionStrategy = null;

            dbExecutionStrategy = EfCfDbConfiguration.IsExecutionStrategySuspended ? (IDbExecutionStrategy)new MySqlExecutionStrategy() : new MySqlEfCfExecutionStrategy(logger);

            // works
            //this.SetExecutionStrategy(MySqlProviderInvariantName.ProviderName,
            //                          () => EfCfDbConfiguration.IsExecutionStrategySuspended ? (IDbExecutionStrategy)new DefaultExecutionStrategy() : new MySqlExecutionStrategy());

            // also works. Is it correct? Does I need to make a class that inherits from MySqlExecutionStrategy() to actually have 'retry' logic? How to test if MySqlEfCfExecutionStrategy will get called and work?
            this.SetExecutionStrategy(MySqlProviderInvariantName.ProviderName,
                                      () => EfCfDbConfiguration.IsExecutionStrategySuspended ? (IDbExecutionStrategy)new DefaultExecutionStrategy() : new MySqlEfCfExecutionStrategy(logger));

            this.DbExecutionStrategy = dbExecutionStrategy;
        }
    }
}
