using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Logging.Interfaces;

namespace EfCfRepoCoverLib.ConnectionResiliency
{
    public class SqliteEfCfExecutionStrategy : EfCfExecutionStrategy
    {
        #region Constructors
        /// <summary>Note: Default 'max retry' count = 5 (total time spent between retries is approximately 26 seconds, plus the 'random' factor: min(random(1, 1.1) * (2 ^ retryCount - 1), maxDelay)).</summary>
        /// <remarks>Relevant info: https://msdn.microsoft.com/en-us/library/system.data.entity.infrastructure.dbexecutionstrategy(v=vs.113).aspx </remarks>
        public SqliteEfCfExecutionStrategy()
        {
        }

        /// <summary>Note: Default 'max retry' count = 5 (total time spent between retries is approximately 26 seconds, plus the 'random' factor: min(random(1, 1.1) * (2 ^ retryCount - 1), maxDelay)).</summary>
        /// <param name="logger">Object that implements the ILogging interface.</param>
        /// <remarks>Relevant info: https://msdn.microsoft.com/en-us/library/system.data.entity.infrastructure.dbexecutionstrategy(v=vs.113).aspx </remarks>
        public SqliteEfCfExecutionStrategy(ILogging logger = null): base(logger)
        {
        }

        /// <summary>DbExecutionStrategy implemenentation that specifies 'max retry count' and 'max delay' values.</summary>
        /// <param name="maxRetryCount">Maximum number of retry attempts.</param>
        /// <param name="maxdelay">Maximum delay (in milliseconds) between retry attempts.</param>
        /// <param name="logger">Object that implements the ILogging interface.</param>
        public SqliteEfCfExecutionStrategy(int maxRetryCount, TimeSpan maxdelay, ILogging logger = null) : base(maxRetryCount, maxdelay, logger)
        {
        }
        #endregion Constructors

        #region IDbExecutionStrategy Method Implementation
        /// <summary>Implemented IDbExecutionStrategy method to evaluate a list of 'retry' Sql Error Numbers to determinee whether an entity framework operation should be 'retried' (or not).</summary>
        /// <param name="exception">Exception that occurred during an entity framework operation.</param>
        /// <returns>True/False flag indicating whether an entity framework operation should be 'retried' (or not).</returns>
        protected override bool ShouldRetryOn(Exception exception)
        {
            var shouldRetry = ShouldRetryOnException(exception);

            return shouldRetry;
        }
        #endregion IDbExecutionStrategy Method Implementation

        public override bool ShouldRetryOnException(Exception exception)
        {
            var shouldRetry = false;

            if (exception is TimeoutException) // If 'exception' is 'TimeoutException', no point in continuing further - we want to retry; 'early return' here.
            {
                if (this.Logger != null) { this.Logger.Info("Retrying in ExecutionStrategy for TimeoutException."); }
                return true;
            }

            var sqlException = exception as SqlException;
            if (sqlException == null) { return shouldRetry; } // If 'exception' can't be cast as 'SqlException', no point in continuing; 'early return' here.

            var sqlErrorNumbersToRetryList = GetSqlErrorNumbersToRetryList(); // Get list of sql error numbers to 'retry on' (e.g. Timeout = 2, Deadlock = 1205).

            // Determine if any of the 'sql error numbers to retry' exist for 'SqlError.Number' in the collection of 'SqlErrors'.
            // If a 'retry' condition is found, set return value flag to 'true'; otherwise, log the 'SqlError.Number' for analysis/troubleshooting.
            var sqlExceptionErrors = sqlException.Errors.Cast<SqlError>();
            var sqlErrorNumbersToRetry = sqlExceptionErrors.Where(sqlError => sqlErrorNumbersToRetryList.Contains(sqlError.Number)).ToList();
            if (sqlErrorNumbersToRetry.Any())
            {
                shouldRetry = true;

                // Create (and log) delimited list of Sql Error Number(s) that caused 'retry' to occur (for analysis/troubleshooting).
                var sqlErrorNumbersToRetryLabel = string.Join(",", sqlErrorNumbersToRetry);
                var logMsg = string.Format("Retrying for sql exception containing sql error number(s): {0} (evaluate for possible addition to error number retry list).", sqlErrorNumbersToRetryLabel);
                if (this.Logger != null) { this.Logger.Info(logMsg); }
            }
            else
            {
                if (this.Logger != null) { this.Logger.Error("Error", sqlException); }

                // The 'ToString' method will recursively provide all nested exception information (including SqlErrors in SqlErrorCollection we want for SqlError.Number evaluation.
                //    The 'SqlError.Number' values can be reviewed and added to 'SqlErrorNumbersToRetryList' for 'retry', if applicable.
                if (this.Logger != null) { this.Logger.Info(sqlException.ToString()); }
            }

            return shouldRetry;
        }

        /// <summary>Retrieves a list of Sql Error Number values that should cause a 'retry' (e.g. Timeout = -2, Deadlock = 1205).</summary>
        /// <returns>A list of Sql Error Number values that should cause a 'retry'.</returns>
        protected override List<int> GetSqlErrorNumbersToRetryList()
        {
            // https://www.sqlite.org/rescode.html (SQLite site: "Result and Error Codes")

            // 5 = SQLITE_BUSY	The SQLITE_BUSY result code indicates that the database file could not be written (or in some cases read) because of concurrent activity by some other database connection, usually a database connection in a separate process.

            // 6 = SQLITE_LOCKED The SQLITE_LOCKED result code indicates that a write operation could not continue because of a conflict within the same database connection or a conflict with a different database connection that uses a shared cache.

            // 10 = SQLITE_IOERR The SQLITE_IOERR result code says that the operation could not finish because the operating system reported an I/O error.

            // 261 = SQLITE_LOCKED_SHAREDCACHE The SQLITE_LOCKED_SHAREDCACHE error code is an extended error code for SQLITE_LOCKED indicating that the locking conflict has occurred due to contention with a different database connection that happens to hold a shared cache with the database connection to which the error was returned. For example, if the other database connection is holding an exclusive lock on the database, then the database connection that receives this error will be unable to read or write any part of the database file unless it has the read_uncommitted pragma enabled.

            // 517 = SQLITE_BUSY_SNAPSHOT The SQLITE_BUSY_SNAPSHOT error code is an extended error code for SQLITE_BUSY that occurs on WAL mode databases when a database connection tries to promote a read transaction into a write transaction but finds that another database connection has already written to the database and thus invalidated prior reads.

            // 3850 = SQLITE_IOERR_LOCK The SQLITE_IOERR_LOCK error code is an extended error code for SQLITE_IOERR indicating an I/O error in the advisory file locking logic. Usually an SQLITE_IOERR_LOCK error indicates a problem obtaining a PENDING lock. However it can also indicate miscellaneous locking errors on some of the specialized VFSes used on Macs.

            // 4106 = SQLITE_IOERR_CLOSE The SQLITE_IOERR_ACCESS error code is an extended error code for SQLITE_IOERR indicating an I/O error within the xClose method on the sqlite3_io_methods object.

            var sqlErrorNumbersToRetry = new List<int>
            {
                5,
                6,
                10,
                261,
                517,
                3850,
                4106
            };

            return sqlErrorNumbersToRetry;
        }
    }
}
