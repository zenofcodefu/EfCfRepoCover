using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Logging.Interfaces;
using MySql.Data.Entity;

namespace EfCfRepoCoverLib.ConnectionResiliency
{
    //public class MySqlEfCfExecutionStrategy : EfCfExecutionStrategy
    public class MySqlEfCfExecutionStrategy : EfCfExecutionStrategy
    {
        #region Constructors
        /// <summary>Note: Default 'max retry' count = 5 (total time spent between retries is approximately 26 seconds, plus the 'random' factor: min(random(1, 1.1) * (2 ^ retryCount - 1), maxDelay)).</summary>
        /// <remarks>Relevant info: https://msdn.microsoft.com/en-us/library/system.data.entity.infrastructure.dbexecutionstrategy(v=vs.113).aspx </remarks>
        public MySqlEfCfExecutionStrategy()
        {
        }

        /// <summary>Note: Default 'max retry' count = 5 (total time spent between retries is approximately 26 seconds, plus the 'random' factor: min(random(1, 1.1) * (2 ^ retryCount - 1), maxDelay)).</summary>
        /// <param name="logger">Object that implements the ILogging interface.</param>
        /// <remarks>Relevant info: https://msdn.microsoft.com/en-us/library/system.data.entity.infrastructure.dbexecutionstrategy(v=vs.113).aspx </remarks>
        //public MySqlEfCfExecutionStrategy(ILogging logger = null): base(logger)
        public MySqlEfCfExecutionStrategy(ILogging logger = null)
        {
        }

        /// <summary>DbExecutionStrategy implemenentation that specifies 'max retry count' and 'max delay' values.</summary>
        /// <param name="maxRetryCount">Maximum number of retry attempts.</param>
        /// <param name="maxdelay">Maximum delay (in milliseconds) between retry attempts.</param>
        /// <param name="logger">Object that implements the ILogging interface.</param>
        public MySqlEfCfExecutionStrategy(int maxRetryCount, TimeSpan maxdelay, ILogging logger = null) : base(maxRetryCount, maxdelay, logger)
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
            // 1040 = Error 1040	08004	ER_CON_COUNT_ERROR	Too many connections

            // 1041 = Error 1041	HY000	ER_OUT_OF_RESOURCES	Out of memory; check if mysqld or some other process uses all available memory; if not, you may have to use 'ulimit' to allow mysqld to use more memory or you can add more swap space

            // 1158 = Error 1158    08S01 ER_NET_READ_ERROR   Got an error reading communication packets
            // 1159 = Error 1159    08S01 ER_NET_READ_INTERRUPTED Got timeout reading communication packets
            // 1160 = Error 1160    08S01 ER_NET_ERROR_ON_WRITE   Got an error writing communication packets
            // 1161 = Error 1161    08S01 ER_NET_WRITE_INTERRUPTED    Got timeout writing communication packets

            // 1205 =  Error: 1205 SQLSTATE: HY000 (ER_LOCK_WAIT_TIMEOUT) Message: Lock wait timeout exceeded; try restarting transaction

            // 1213 = Error: 1213 SQLSTATE: 40001 (ER_LOCK_DEADLOCK) Message: Deadlock found when trying to get lock; try restarting transaction

            // 1613 =  Error: 1613 SQLSTATE: XA106 (ER_XA_RBTIMEOUT) Message: XA_RBTIMEOUT: Transaction branch was rolled back: took too long

            // 1614 = Error: 1614 SQLSTATE: XA102 (ER_XA_RBDEADLOCK) Message: XA_RBDEADLOCK: Transaction branch was rolled back: deadlock was detected

            // 3024 = Error: 3024 SQLSTATE: HY000 (ER_QUERY_TIMEOUT) Message: Query execution was interrupted, maximum statement execution time exceeded

            // 3058 = Error: 3058 SQLSTATE: HY000 (ER_USER_LOCK_DEADLOCK) Message: Deadlock found when trying to get user - level lock; try rolling back transaction / releasing locks and restarting lock acquisition. This error is returned when the metdata locking subsystem detects a deadlock for an attempt to acquire a named lock with GET_LOCK.

            // 3132 = Error: 3132 SQLSTATE: HY000 (ER_LOCKING_SERVICE_DEADLOCK) Message: Deadlock found when trying to get locking service lock; try releasing locks and restarting lock acquisition. ER_LOCKING_SERVICE_DEADLOCK was added in 5.7.8.

            // 3133 = Error: 3133 SQLSTATE: HY000 (ER_LOCKING_SERVICE_TIMEOUT) Message: Service lock wait timeout exceeded. ER_LOCKING_SERVICE_TIMEOUT was added in 5.7.8

            // 3168 = Error: 3168 SQLSTATE: HY000 (ER_SERVER_ISNT_AVAILABLE) Message: Server isn't available ER_SERVER_ISNT_AVAILABLE was added in 5.7.9.

            var sqlErrorNumbersToRetry = new List<int>
            {
                1040,
                1041,
                1158,
                1159,
                1160,
                1161,
                1205,
                1213,
                1613,
                1614,
                3024,
                3058,
                3132,
                3133,
                3168
            };

            return sqlErrorNumbersToRetry;
        }

        public new void Execute(Action operation)
        {
            base.Execute(operation);
        }
    }
}
