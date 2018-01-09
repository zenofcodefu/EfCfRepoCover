using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using EfCfRepoCoverLib.CustomErrors;
using Logging.Interfaces;

namespace EfCfRepoCoverLib
{
    public static class ConfigurationUtility
    {
        public static DbConfigurationDatabaseType GetDbConfigurationDatabaseType()
        {
            var dbConfigurationDatabaseTypeFromConfig = DbConfigurationDatabaseType.MsSqlServer;

            dbConfigurationDatabaseTypeFromConfig = GetDbConfigurationDatabaseTypeValueFromConfiguration();

            return dbConfigurationDatabaseTypeFromConfig;
        }

        private static DbConfigurationDatabaseType GetDbConfigurationDatabaseTypeValueFromConfiguration()
        {
            var dbConfigurationDatabaseTypeFromConfig = DbConfigurationDatabaseType.MsSqlServer;

            var entityFrameworkFriendlyProviderNameValue = GetEntityFrameworkFriendlyProviderNameFromConfiguration(); // Get value from config file (if not found, empty string returned).

            dbConfigurationDatabaseTypeFromConfig = GetDbConfigurationTypeByString(entityFrameworkFriendlyProviderNameValue); // Get enumeration value based on string (if no match, Ms Sql Server used by default).

            if (dbConfigurationDatabaseTypeFromConfig.Equals(DbConfigurationDatabaseType.Unknown))
            {
                var entityFrameworkFriendlyProviderNameKey = Constants.ENTITY_FRAMEWORK_FRIENDLY_PROVIDER_NAME_KEY;
                var dbConfigurationDatabaseTypeNameList = GetDbConfigurationDatabaseTypeValidNameList();
                var supportedDbTypesForConfigurationLabel = string.Join(" | ", dbConfigurationDatabaseTypeNameList);

                var message = string.Format("Required config file key/value element in <appSettings> child element is missing or has an invalid value (e.g. <add key=\"{0}\" value=\"MsSqlServer\" />).{1}" +
                                            "Valid values for the 'value=\"\"' attribute are: [{2}]. ", entityFrameworkFriendlyProviderNameKey, Environment.NewLine, supportedDbTypesForConfigurationLabel);

                throw new ConfigurationMissingException(message);
            }

            return dbConfigurationDatabaseTypeFromConfig;
        }

        private static string GetEntityFrameworkFriendlyProviderNameFromConfiguration()
        {
            var entityFrameworkFriendlyProviderNameKey = Constants.ENTITY_FRAMEWORK_FRIENDLY_PROVIDER_NAME_KEY;

            var entityFrameworkFriendlyProviderNameValue = ConfigurationManager.AppSettings[entityFrameworkFriendlyProviderNameKey] ?? string.Empty;

            return entityFrameworkFriendlyProviderNameValue;
        }

        public static bool IsDatabaseProviderSameInConfiguration(string connectionStringName, DbConfigurationDatabaseType dbConfigurationDatabaseTypeValue, ILogging logger = null)
        {
            var isDatabaseProviderSameInConfiguration = false;

            var dbConfigurationDatabaseTypeFromConfig = GetDbConfigurationTypeByConnectionStringName(connectionStringName, logger);

            // Does the 'DbConfigurationDatabaseType' specified in config AppSettings (e.g. '<add key="entityFrameworkFriendlyProviderName" value="MsSqlServer" />')
            // match the specified 'DbConfigurationDatabaseType' (e.g. 'MsSqlServer' was specified in AppSettings, but 'ProviderName="System.Data.SQLite"' is specified in connection string)?
            isDatabaseProviderSameInConfiguration = dbConfigurationDatabaseTypeValue.Equals(dbConfigurationDatabaseTypeFromConfig);

            return isDatabaseProviderSameInConfiguration;
        }

        public static DbConfigurationDatabaseType GetDbConfigurationTypeByConnectionStringName(string connectionStringName, ILogging logger= null)
        {
            var dbConfigurationDatabaseType = DbConfigurationDatabaseType.Unknown;

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName]; // Attempt to find connection string in configuration file.
            if (connectionStringSettings == null)
            {
                if (logger != null) { logger.WarnFormat("No connection string with name='{0}' could be found.", connectionStringName); }

                return dbConfigurationDatabaseType;
            }

            var configConnectionStringProviderName = connectionStringSettings.ProviderName; // Attempt to find providerName attribute value from matching connection string.
            if (string.IsNullOrEmpty(configConnectionStringProviderName))
            {
                if (logger != null) { logger.WarnFormat("No providerName attribute value was found for connection string with name='{0}'.", connectionStringName); }

                return dbConfigurationDatabaseType;
            }

            // Try to match 'providerName' value found to enum 'DbConfigurationDatabaseType'.
            dbConfigurationDatabaseType = GetDbConfigurationTypeByProviderName(configConnectionStringProviderName);

            return dbConfigurationDatabaseType;
        }

        public static DbConfigurationDatabaseType GetDbConfigurationTypeByString(string dbConfigurationDatabaseValue)
        {
            var dbConfigurationDatabaseType = DbConfigurationDatabaseType.Unknown;

            dbConfigurationDatabaseValue = dbConfigurationDatabaseValue.ToUpper();

            switch (dbConfigurationDatabaseValue)
            {
                case Constants.DB_CONFIGURATION_DATABASE_TYPE_MSSQLSERVER: // (i.e. "MSSQLSERVER").
                    dbConfigurationDatabaseType = DbConfigurationDatabaseType.MsSqlServer;
                    break;

                case Constants.DB_CONFIGURATION_DATABASE_TYPE_MYSQL: // (i.e. "MYSQL").
                    dbConfigurationDatabaseType = DbConfigurationDatabaseType.MySql;
                    break;

                case Constants.DB_CONFIGURATION_DATABASE_TYPE_MARIADB: // (i.e. "MARIADB").
                    //dbConfigurationDatabaseType = DbConfigurationDatabaseType.MariaDb; // Setting to 'MySql' until 'MariaDb' uses separate/different provider dlls different than 'MySql'.
                    dbConfigurationDatabaseType = DbConfigurationDatabaseType.MySql;
                    break;

                case Constants.DB_CONFIGURATION_DATABASE_TYPE_SQLITE: // (i.e. "SQLITE").
                    dbConfigurationDatabaseType = DbConfigurationDatabaseType.Sqlite;
                    break;

                default:
                    dbConfigurationDatabaseType = DbConfigurationDatabaseType.Unknown;
                    break;
            }

            return dbConfigurationDatabaseType;
        }

        public static DbConfigurationDatabaseType GetDbConfigurationTypeByProviderName(string dbConfigurationDatabaseValue)
        {
            var dbConfigurationDatabaseType = DbConfigurationDatabaseType.Unknown;

            switch (dbConfigurationDatabaseValue)
            {
                case Constants.PROVIDER_INVARIANTNAME_MSSQLSERVER: // (i.e. "System.Data.SqlClient")
                    dbConfigurationDatabaseType = DbConfigurationDatabaseType.MsSqlServer;
                    break;

                case Constants.PROVIDER_INVARIANTNAME_MYSQL: // (i.e. "MySql.Data.MySqlClient")
                    dbConfigurationDatabaseType = DbConfigurationDatabaseType.MySql;
                    break;

                case Constants.PROVIDER_INVARIANTNAME_SQLITE: // (i.e. "System.Data.SQLite")
                    dbConfigurationDatabaseType = DbConfigurationDatabaseType.Sqlite;
                    break;

                default:
                    dbConfigurationDatabaseType = DbConfigurationDatabaseType.Unknown;
                    break;
            }

            return dbConfigurationDatabaseType;
        }

        public static List<string> GetDbConfigurationDatabaseTypeValidNameList()
        {
            var enumType = typeof(DbConfigurationDatabaseType);

            var dbConfigurationDatabaseTypeNameList = Enum.GetNames(enumType).ToList();

            dbConfigurationDatabaseTypeNameList.Remove("Unknown"); // Remove 'Unknown', since this is not a 'valid' database configuration database type.

            return dbConfigurationDatabaseTypeNameList;
        }

        public enum DbConfigurationDatabaseType
        {
            Unknown = -1,
            MsSqlServer = 0,
            MySql = 1,
            MariaDb = 2,
            Sqlite = 3
        }
    }
}
