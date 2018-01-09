using System;
using Logging.Interfaces;

namespace EfCfRepoCoverLib.ConnectionResiliency
{
    public class ExecutionStrategyUtility
    {
        public static EfCfExecutionStrategy GetExecutionStrategy(ILogging logger = null)
        {
            EfCfExecutionStrategy efCfExecutionStrategy = null;

            var dbConfigurationDatabaseType = ConfigurationUtility.GetDbConfigurationDatabaseType();

            switch (dbConfigurationDatabaseType)
            {
                case ConfigurationUtility.DbConfigurationDatabaseType.MsSqlServer:
                    efCfExecutionStrategy = new MsSqlServerEfCfExecutionStrategy(logger);
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.MySql:
                    efCfExecutionStrategy = new MySqlEfCfExecutionStrategy(logger);
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.MariaDb:
                    efCfExecutionStrategy = new MySqlEfCfExecutionStrategy(logger);
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.Sqlite:
                    efCfExecutionStrategy = new SqliteEfCfExecutionStrategy(logger);
                    break;

                default:
                    efCfExecutionStrategy = new MsSqlServerEfCfExecutionStrategy(logger);
                    break;
            }

            return efCfExecutionStrategy;
        }
    }
}
