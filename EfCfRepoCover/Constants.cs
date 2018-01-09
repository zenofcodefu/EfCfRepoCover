using System;

namespace EfCfRepoCoverLib
{
    public class Constants
    {
        public const string PROVIDER_INVARIANTNAME_MSSQLSERVER = "System.Data.SqlClient";
        public const string PROVIDER_INVARIANTNAME_MYSQL = "MySql.Data.MySqlClient";
        public const string PROVIDER_INVARIANTNAME_SQLITE = "System.Data.SQLite";

        public const string LOGICAL_CALL_CONTEXT_OBJECT_NAME = "SuspendExecutionStrategy";

        public const string DB_CONFIGURATION_DATABASE_TYPE_MSSQLSERVER = "MSSQLSERVER";
        public const string DB_CONFIGURATION_DATABASE_TYPE_MYSQL = "MYSQL";
        public const string DB_CONFIGURATION_DATABASE_TYPE_MARIADB = "MARIADB";
        public const string DB_CONFIGURATION_DATABASE_TYPE_SQLITE = "SQLITE";

        public const string ENTITY_FRAMEWORK_FRIENDLY_PROVIDER_NAME_KEY = "entityFrameworkFriendlyProviderName";
    }
}
