using System;
using System.IO;
using System.Reflection;
using EfCfRepoCoverLib;

namespace EfCfRepoCoverTests
{
    public static class UtilGeneral
    {
        public static string GetFullyQualifiedConfigFileNameByDbConfigurationDatabaseType(ConfigurationUtility.DbConfigurationDatabaseType dbConfigurationDatabaseTypeValue)
        {
            var configBaseDirectory = AssemblyDirectory();

            var configFileName = GetConfigFileNameByDbConfigurationDatabaseType(dbConfigurationDatabaseTypeValue);

            var fullyQualifiedConfigFileName = Path.Combine(configBaseDirectory, configFileName);

            return fullyQualifiedConfigFileName;
        }

        public static string AssemblyDirectory()
        {
            var codeBaseLocation = Assembly.GetExecutingAssembly().CodeBase;

            var codeBaseLocationUriBuilder = new UriBuilder(codeBaseLocation);

            var codeBaseLocationPath = Uri.UnescapeDataString(codeBaseLocationUriBuilder.Path);

            var directoryName = Path.GetDirectoryName(codeBaseLocationPath);

            return directoryName;
        }

        public static string GetConfigFileNameByDbConfigurationDatabaseType(ConfigurationUtility.DbConfigurationDatabaseType dbConfigurationDatabaseTypeValue)
        {
            var configFileName = string.Empty;

            switch (dbConfigurationDatabaseTypeValue)
            {
                case ConfigurationUtility.DbConfigurationDatabaseType.MsSqlServer:
                    configFileName = Constants.DB_CONFIGURATION_DATABASE_TYPE_MSSQLSERVER_FILE_NAME;
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.MariaDb:
                    configFileName = Constants.DB_CONFIGURATION_DATABASE_TYPE_MARIADB_FILE_NAME;
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.MySql:
                    configFileName = Constants.DB_CONFIGURATION_DATABASE_TYPE_MYSQL_FILE_NAME;
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.Sqlite:
                    configFileName = Constants.DB_CONFIGURATION_DATABASE_TYPE_SQLITE_FILE_NAME;
                    break;
            }

            return configFileName;
        }
    }
}
