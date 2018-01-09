using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Logging.Interfaces;

namespace EfCfRepoCoverLib.ConnectionResiliency
{
    public class MsSqlServerEfCfExecutionStrategy: EfCfExecutionStrategy
    {
        #region Constructors
        /// <summary>Note: Default 'max retry' count = 5 (total time spent between retries is approximately 26 seconds, plus the 'random' factor: min(random(1, 1.1) * (2 ^ retryCount - 1), maxDelay)).</summary>
        /// <remarks>Relevant info: https://msdn.microsoft.com/en-us/library/system.data.entity.infrastructure.dbexecutionstrategy(v=vs.113).aspx </remarks>
        public MsSqlServerEfCfExecutionStrategy()
        {
        }

        ///// <summary>Note: Default 'max retry' count = 5 (total time spent between retries is approximately 26 seconds, plus the 'random' factor: min(random(1, 1.1) * (2 ^ retryCount - 1), maxDelay)).</summary>
        ///// <param name="logger">Object that implements the ILogging interface.</param>
        ///// <remarks>Relevant info: https://msdn.microsoft.com/en-us/library/system.data.entity.infrastructure.dbexecutionstrategy(v=vs.113).aspx </remarks>
        public MsSqlServerEfCfExecutionStrategy(ILogging logger = null) : base(logger)
        {
        }

        /// <summary>DbExecutionStrategy implemenentation that specifies 'max retry count' and 'max delay' values.</summary>
        /// <param name="maxRetryCount">Maximum number of retry attempts.</param>
        /// <param name="maxdelay">Maximum delay (in milliseconds) between retry attempts.</param>
        /// <param name="logger">Object that implements the ILogging interface.</param>
        public MsSqlServerEfCfExecutionStrategy(int maxRetryCount, TimeSpan maxdelay, ILogging logger = null) : base(maxRetryCount, maxdelay, logger)
        {
        }
        #endregion Constructors

        #region IDbExecutionStrategy Method Implementation
        /// <summary>Implemented IDbExecutionStrategy method to evaluate a list of 'retry' Sql Error Numbers to determine whether an entity framework operation should be 'retried' (or not).</summary>
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
            // -1 = Timeout
            // -2 = Timeout
            //  2 = Server not found or Inaccessible
            //  1205 = Deadlock

            var sqlErrorNumbersToRetry = new List<int>
            {
                -1,
                -2,
                2,
                1205
            };

            return sqlErrorNumbersToRetry;
        }
    }
}
