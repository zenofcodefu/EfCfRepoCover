using System;
using EfCfRepoCoverLib;
using Logging.Interfaces;

namespace EfCfRepoCoverExamples.Repository.YourDatabaseNameHere
{
    public class YourDatabaseNameRepository : EfCfBaseRepository
    {
        private YourDatabaseNameDbContext YourDatabaseNameDbContext { get; set; }

        #region Constructors
        public YourDatabaseNameRepository(bool isThrowErrorsEnabled = true, ILogging logger = null, bool isLoggingVerbose = false, bool isSqlLoggingEnabled = false)
            : base(new YourDatabaseNameDbContext(), isThrowErrorsEnabled, logger, isLoggingVerbose, isSqlLoggingEnabled)
        {
            this.YourDatabaseNameDbContext = (YourDatabaseNameDbContext)this.EfCfBaseDbContext;
        }

        public YourDatabaseNameRepository(string connectionStringName, bool isThrowErrorsEnabled = true, ILogging logger = null, bool isLoggingVerbose = false, bool isSqlLoggingEnabled = false)
            : base(new YourDatabaseNameDbContext(connectionStringName), isThrowErrorsEnabled, logger, isLoggingVerbose, isSqlLoggingEnabled)
        {
            this.YourDatabaseNameDbContext = (YourDatabaseNameDbContext)this.EfCfBaseDbContext;
        }
        #endregion Constructors
    }
}
