using System;
using System.Data.Entity;
using EfCfRepoCoverLib.CustomErrors;

namespace EfCfRepoCoverLib.DbConfigurations
{
    public class DbConfigurationUtility
    {
        public static void InitializeEfDbConfiguration()
        {
            var dbConfiguration = DbConfigurationUtility.GetDbConfiguration();
            SetDbConfiguration(dbConfiguration);
        }

        private static void SetDbConfiguration(EfCfDbConfiguration efCfDbConfiguration)
        {
            DbConfiguration.SetConfiguration(efCfDbConfiguration); // Used to allow a non-static logger for Entity Framework 'ExecutionStrategy.ShouldRetryOn()' method logging.

            var efCfDbConfigurationLabel = efCfDbConfiguration.ToString();
            var traceMsg = string.Format("SetDbConfiguration() called DbConfiguration.SetConfiguration({0}).", efCfDbConfigurationLabel);
            System.Diagnostics.Trace.WriteLine(traceMsg);
        }

        public static EfCfDbConfiguration GetDbConfiguration()
        {
            EfCfDbConfiguration dbConfiguration = null;

            var dbConfigurationDatabaseType = ConfigurationUtility.GetDbConfigurationDatabaseType();

            switch (dbConfigurationDatabaseType)
            {
                case ConfigurationUtility.DbConfigurationDatabaseType.MsSqlServer:
                    dbConfiguration = new MsSqlServerEfCfDbConfiguration();
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.MySql:
                    dbConfiguration = new MySqlEfCfDbConfiguration();
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.MariaDb:
                    dbConfiguration = new MySqlEfCfDbConfiguration();
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.Sqlite:
                    dbConfiguration = new SqliteEfCfDbConfiguration();
                    break;

                default:
                    dbConfiguration = new MsSqlServerEfCfDbConfiguration();
                    break;
            }

            return dbConfiguration;
        }

        public static EfCfDbConfiguration GetDbConfigurationByDatabaseTypeEnum(ConfigurationUtility.DbConfigurationDatabaseType dbConfigurationDatabaseTypeValue)
        {
            EfCfDbConfiguration dbConfiguration = null;

            switch (dbConfigurationDatabaseTypeValue)
            {
                case ConfigurationUtility.DbConfigurationDatabaseType.MsSqlServer:
                    dbConfiguration = new MsSqlServerEfCfDbConfiguration();
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.MySql:
                    dbConfiguration = new MySqlEfCfDbConfiguration();
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.MariaDb:
                    dbConfiguration = new MySqlEfCfDbConfiguration();
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.Sqlite:
                    dbConfiguration = new SqliteEfCfDbConfiguration();
                    break;

                default:
                    dbConfiguration = new MsSqlServerEfCfDbConfiguration();
                    break;
            }

            return dbConfiguration;
        }
    }
}
