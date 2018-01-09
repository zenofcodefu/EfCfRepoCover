using System;

namespace EfCfRepoCoverLib.CustomErrors
{
    [Serializable]
    public class DbProviderTypeMismatchException: Exception
    {
        // This exception will occur when the 'Db Type' specified in config AppSettings (e.g. '<add key="entityFrameworkFriendlyProviderName" value="MsSqlServer" />')
        // does not match the 'Db Type' in the connection string being used.
        // (e.g. 'MsSqlServer' was specified in AppSettings, but 'ProviderName="System.Data.SQLite"' is specified in connection string).

        /// <summary>String 'label' for database 'type' determined when the DbContext was created (e.g. 'MsSqlServer').</summary>
        public string DbContextDbTypeLabel { get; private set; }

        /// <summary>String 'label' for the database 'type' specified in the 'appSettings' section (e.g. 'add key="entityFrameworkFriendlyProviderName" value="MsSqlServer" ').</summary>
        public string ConfigurationtDbTypeLabel { get; private set; }

        public DbProviderTypeMismatchException(string message, string dbContextDbTypeLabel, string configurationtDbTypeLabel) : base(message)
        {
            this.DbContextDbTypeLabel = dbContextDbTypeLabel;
            this.ConfigurationtDbTypeLabel = configurationtDbTypeLabel;
        }
    }
}
