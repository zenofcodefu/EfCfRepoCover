using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using Logging.Interfaces;

namespace EfCfRepoCoverLib.ConnectionResiliency
{
    public abstract class EfCfExecutionStrategy: DbExecutionStrategy
    {
        #region Constants
        /// <summary>Default value for the maximum number of retry attempts.</summary>
        public const int MAX_RETRY_COUNT_DEFAULT = 5;

        /// <summary>Default value for the maximum delay in milliseconds between retries.</summary>
        public const int MAX_DELAY_SECONDS_DEFAULT = 26;
        #endregion Constants

        #region Public Properties
        /// <summary>Class that implements ILogging interface (to allow logging of retry attempts, errors, diagnostic information, etc).</summary>
        public ILogging Logger { get; private set; }

        /// <summary>The maximum number of retry attempts.</summary>
        public int MaxRetryCount { get; private set; }

        /// <summary>The maximum delay in milliseconds between retries.</summary>
        public TimeSpan MaxDelay { get; private set; }
        #endregion Public Properties

        #region Constructors
        /// <summary>Note: Default 'max retry' count = 5 (total time spent between retries is approximately 26 seconds, plus the 'random' factor: min(random(1, 1.1) * (2 ^ retryCount - 1), maxDelay)).</summary>
        /// <remarks>Relevant info: https://msdn.microsoft.com/en-us/library/system.data.entity.infrastructure.dbexecutionstrategy(v=vs.113).aspx </remarks>
        protected EfCfExecutionStrategy()
        {
            this.Logger = null;
            this.MaxRetryCount = MAX_RETRY_COUNT_DEFAULT;
            this.MaxDelay = TimeSpan.FromSeconds(MAX_DELAY_SECONDS_DEFAULT);
        }

        /// <summary>Note: Default 'max retry' count = 5 (total time spent between retries is approximately 26 seconds, plus the 'random' factor: min(random(1, 1.1) * (2 ^ retryCount - 1), maxDelay)).</summary>
        /// <param name="logger">Object that implements the ILogging interface.</param>
        /// <remarks>Relevant info: https://msdn.microsoft.com/en-us/library/system.data.entity.infrastructure.dbexecutionstrategy(v=vs.113).aspx </remarks>
        protected EfCfExecutionStrategy(ILogging logger = null)
        {
            this.Logger = logger;
            this.MaxRetryCount = MAX_RETRY_COUNT_DEFAULT;
            this.MaxDelay = TimeSpan.FromSeconds(MAX_DELAY_SECONDS_DEFAULT);
        }

        /// <summary>DbExecutionStrategy implemenentation that specifies 'max retry count' and 'max delay' values.</summary>
        /// <param name="maxRetryCount">Maximum number of retry attempts.</param>
        /// <param name="maxdelay">Maximum delay (in milliseconds) between retry attempts.</param>
        /// <param name="logger">Object that implements the ILogging interface.</param>
        protected EfCfExecutionStrategy(int maxRetryCount, TimeSpan maxdelay, ILogging logger = null) : base(maxRetryCount, maxdelay)
        {
            this.Logger = logger;
            this.MaxRetryCount = maxRetryCount;
            this.MaxDelay = maxdelay;
        }
        #endregion Constructors

        #region IDbExecutionStrategy Method Implementation
        public abstract bool ShouldRetryOnException(Exception exception);

        #endregion IDbExecutionStrategy Method Implementation

        protected abstract List<int> GetSqlErrorNumbersToRetryList();

        public new void Execute(Action operation)
        {
            base.Execute(operation);
        }
    }
}
