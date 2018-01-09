using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using EfCfRepoCoverLib.ConnectionResiliency;
using EfCfRepoCoverLib.CustomErrors;
using EfCfRepoCoverLib.DbConfigurations;
using Logging.Interfaces;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace EfCfRepoCoverLib
{
    /// <summary>EfCf (Entity Framework Code First) Base Repository. 
    /// Provides EF 'Code First' CRUD methods using generic, POCO classes (that represent/model db tables) via a DbContext derived class ('EfCfBaseDbContext').</summary>

    #region General Info and High Level Example Usage
    //
    // What is the point in creating a class that essentially just calls Entity Framework (Code First) CRUD (Create, Read, Update, Delete) methods?
    //     1) You can specify any table (poco class) in a database (DbContext), because the methods are generic (no need to write CRUD methods specific to each table (poco class)).
    //     2) The code to use this class is small, one line for a CRUD operation (e.g. 'var bookFound = base.FindEntity(Book, keyValues, libraryDbContext)').
    //     3) Entity Framework exceptions/errors can be a little tricky to catch the right error type and retrieve the relevant information. This class catches and logs/traces the errors for you.
    //
    // Usage:
    //     General: Create a 'EfCfBaseDbContext' class (represents database) containing 'DbSet<>' properties (each property is a table in db), then pass 'DbContext' class to 'EfCfBaseRepository' and use CRUD methods.
    //
    //     Create a C# class for each table you want to work with in a database (e.g. 'public class Book'). Note: There are EF tools to generate C# classes from existing tables in a database.
    //     Create a 'DbContext' derived class (e.g. 'LibraryDbContext : EfCfBaseDbContext').
    //     Add to that class, a DbSet<> property for each table in the database you want to work with (e.g. 'public virtual DbSet<Book> Book { get; set; }' - for 'Book' table in a database).
    //     
    //     Create a 'EfCfBaseRepository' derived class (e.g. 'LibraryRepository : EfCfBaseRepository'). (You could also just use 'EfCfBaseRepository' directly, but deriving gives you more flexibility).
    //     You're done. Use any desired CRUD methods via the repository.
    //     
    //     Example usage of repository CRUD (Create, Read, Update, Delete) methods:
    //         var bookAdded = LibaryRepository.AddEntity(bookToAdd);              // Create
    //         var bookFound = LibaryRepository.FindEntity(bookToFind, keyValues); // Read
    //         LibaryRepository.UpdateEntity(currentBook, updateBook);             // Update
    //         LibaryRepository.DeleteEntity(deleteBook);                          // Delete
    //
    //         QueryRawSql()                                                       // Sql executed against DbContext database; attempts to map results to specified POCO class
    //         RunTransactionalOperations()                                        // Run 'user-initiated' transactions with 'DbExecutionStrategy', 'retry' logic
    #endregion General Info and High Level Example Usage

    public class EfCfBaseRepository
    {
        #region Constructors
        /// <summary>Constructor that allows a DbContext class that inherits from the 'EfCfBaseDbContext' base class).</summary>
        /// <param name="efCfBaseDbContext">DbContext class that inherits from the 'EfCfBaseDbContext' base class to be used by the repository.</param>
        /// <param name="isThrowErrorsEnabled">Indicates whether errors will be thrown (default is 'true').</param>
        /// <param name="logger">ILogging interface implementation (for logging).</param>
        /// <param name="isLoggingVerbose">Flag indicating whether logging should be very detailed.</param>
        /// <param name="isSqlLoggingEnabled">Flag indicating whether sql logging (with a lot of detail) will occur (e.g. for troubleshooting or analysis).</param>
        public EfCfBaseRepository(EfCfBaseDbContext efCfBaseDbContext, bool isThrowErrorsEnabled = true, ILogging logger = null, bool isLoggingVerbose = false, bool isSqlLoggingEnabled = false)
        {
            this.EfCfBaseDbContext = efCfBaseDbContext;
            this.IsThrowErrorsEnabled = isThrowErrorsEnabled;
            this.IsLoggingVerbose = isLoggingVerbose;
            this.IsSqlLoggingEnabled = isSqlLoggingEnabled;
            this.Logger = logger;
            this.DbConfigurationDatabaseType = ConfigurationUtility.GetDbConfigurationDatabaseType();
            this.EfCfExecutionStrategy = ExecutionStrategyUtility.GetExecutionStrategy(this.Logger);

            ConfirmDbContextDbProviderSameAsSpecifiedConfigDbProvider();

            SetDbContextSqlLogging();
        }

        /// <summary>Constructor that allows a connection string to be specified for use by the 'EfCfBaseDbContext' base class.</summary>
        /// <param name="connectionStringName">Connection string used by the 'EfCfBaseDbContext' base class.</param>
        /// <param name="isThrowErrorsEnabled">Indicates whether errors will be thrown (default is 'true').</param>
        /// <param name="logger">ILogging interface implementation (for logging).</param>
        /// <param name="isLoggingVerbose">Flag indicating whether logging should be very detailed.</param>
        /// <param name="isSqlLoggingEnabled">Flag indicating whether sql logging (with a lot of detail) will occur (e.g. for troubleshooting or analysis).</param>
        public EfCfBaseRepository(string connectionStringName, bool isThrowErrorsEnabled = true, ILogging logger = null, bool isLoggingVerbose = false, bool isSqlLoggingEnabled = false)
        {
            this.EfCfBaseDbContext = new EfCfBaseDbContext(connectionStringName);
            this.IsThrowErrorsEnabled = isThrowErrorsEnabled;
            this.IsLoggingVerbose = isLoggingVerbose;
            this.IsSqlLoggingEnabled = isSqlLoggingEnabled;
            this.Logger = logger;
            this.DbConfigurationDatabaseType = ConfigurationUtility.GetDbConfigurationDatabaseType();
            this.EfCfExecutionStrategy = ExecutionStrategyUtility.GetExecutionStrategy(this.Logger);

            SetDbContextSqlLogging();
        }

        /// <summary>Constructor that allows a connection string to be specified for use by the 'EfCfBaseDbContext' base class.</summary>
        /// <param name="efCfBaseDbContext">DbContext class that inherits from the 'EfCfBaseDbContext' base class to be used by the repository.</param>
        /// <param name="isThrowErrorsEnabled">Indicates whether errors will be thrown (default is 'true').</param>
        /// <param name="logger">ILogging interface implementation (for logging).</param>
        /// <param name="isLoggingVerbose">Flag indicating whether logging should be very detailed.</param>
        /// <param name="isSqlLoggingEnabled">Flag indicating whether sql logging (with a lot of detail) will occur (e.g. for troubleshooting or analysis).</param>
        /// <param name="isConnectionStringInConfig">Flag indicating whether connection string exists in config file (e.g. could be created dynamically).</param>
        public EfCfBaseRepository(EfCfBaseDbContext efCfBaseDbContext, bool isThrowErrorsEnabled = true, ILogging logger = null, bool isLoggingVerbose = false, bool isSqlLoggingEnabled = false, bool isConnectionStringInConfig = true)
        {
            this.EfCfBaseDbContext = efCfBaseDbContext;
            this.IsThrowErrorsEnabled = isThrowErrorsEnabled;
            this.IsLoggingVerbose = isLoggingVerbose;
            this.IsSqlLoggingEnabled = isSqlLoggingEnabled;
            this.Logger = logger;
            this.DbConfigurationDatabaseType = ConfigurationUtility.GetDbConfigurationDatabaseType();
            this.EfCfExecutionStrategy = ExecutionStrategyUtility.GetExecutionStrategy(this.Logger);

            if (isConnectionStringInConfig) // If connection string exists in config file, the 'providerName' can be evaluated to make sure it is consistent with specified key/value provider.
            {
                ConfirmDbContextDbProviderSameAsSpecifiedConfigDbProvider();
            }

            SetDbContextSqlLogging();
        }
        #endregion Constructors

        #region Private Properties
        /// <summary>Logging object that implements the ILogging interface.</summary>
        private ILogging Logger { get; set; }

        /// <summary>Flag indicating whether logging with a lot more detail will occur (e.g. for troubleshooting or analysis).</summary>
        private bool IsLoggingVerbose { get; set; }
        #endregion Private Properties

        #region Public Properties
        /// <summary>DbContext provided by construction (explicitly provided or created based on connection string provided to a constructor).</summary>
        public EfCfBaseDbContext EfCfBaseDbContext { get; private set; }

        /// <summary>Indicates whether errors will be thrown. The default is 'true' and is set on the constructor.</summary>
        public bool IsThrowErrorsEnabled { get; private set; }
        
        /// <summary>Flag indicating whether sql logging (with a lot of detail) will occur (e.g. for troubleshooting or analysis).</summary>
        private bool IsSqlLoggingEnabled { get; set; }

        /// <summary>Enumerated value for supported database types (e.g. 'MS SQL Server', 'MySql', 'MariaDb', 'SQLite').</summary>
        public ConfigurationUtility.DbConfigurationDatabaseType DbConfigurationDatabaseType { get; private set; }

        /// <summary>Specifies 'ExecutionStrategy' (typically used for 'Retry' scenarios/conditions).</summary>
        public EfCfExecutionStrategy EfCfExecutionStrategy { get; private set; }
        #endregion Public Properties

        #region Public CRUD Methods (Create, Read, Update, Delete)
        #region Public Methods Add
        /// <summary>Uses a specified 'DbContext' to create/insert a db record based on the 'addEntity' instance specified.</summary>
        /// <typeparam name="TEntity">POCO class type (representing a db table) in a typed 'DbContext.DbSet' property (e.g. 'public virtual DbSet&lt;Book&gt; Book { get; set; }').</typeparam>
        /// <param name="addEntity">POCO class instance containing data to be added/inserted.</param>
        /// <param name="efCfBaseDbContext">EfCfBaseDbContext (DbContext derived class) used for db insert.</param>
        /// <returns>POCO class containing values from newly created/inserted db record.</returns>
        public TEntity AddEntity<TEntity>(TEntity addEntity, EfCfBaseDbContext efCfBaseDbContext = null) where TEntity : class
        {
            if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

            var addedEntity = default(TEntity);

            try
            {
                //var dbSet = efCfBaseDbContext.GetDbSetByPoco(addEntity); // Get DbSet by class type.
                var dbSet = efCfBaseDbContext.Set<TEntity>(); // Get DbSet by class type.

                addedEntity = dbSet.Add(addEntity);

                efCfBaseDbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                ProcessError(exception);
                if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
            }

            return addedEntity;
        }
        #endregion Public Methods Add

        #region Public Methods Find
        /// <summary>Uses a specified 'DbContext' to find an entity (e.g. db record) based on the 'keyValues' (i.e. primary key) provided.</summary>
        /// <typeparam name="TEntity">POCO class type (representing a db table) in a typed 'DbContext.DbSet' property (e.g. 'public virtual DbSet&lt;Book&gt; Book { get; set; }').</typeparam>
        /// <param name="findEntity">POCO class instance of the Type to be found (e.g. db Table).</param>
        /// <param name="keyValues">Array of key values (usually 1 for primary key) used to find specific entity (i.e. db record).</param>
        /// <param name="efCfBaseDbContext">EfCfBaseDbContext (DbContext derived class) used to find entity/record in db.</param>
        /// <returns>POCO class containing values from db record found (null if nothing found based on 'keyValues' provided).</returns>
        public TEntity FindEntity<TEntity>(TEntity findEntity, object[] keyValues, EfCfBaseDbContext efCfBaseDbContext = null) where TEntity : class
        {
            if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

            var foundEntity = FindEntity<TEntity>(keyValues, efCfBaseDbContext);

            return foundEntity;
        }

        public TEntity FindEntity<TEntity>(object[] keyValues, EfCfBaseDbContext efCfBaseDbContext = null) where TEntity : class
        {
            if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

            var foundEntity = default(TEntity);

            try
            {
                //var dbSet = efCfBaseDbContext.GetDbSetByPoco(findEntity); // Get DbSet by class type.
                var dbSet = efCfBaseDbContext.Set<TEntity>(); // Get DbSet by class type.

                foundEntity = dbSet.Find(keyValues);
            }
            catch (Exception exception)
            {
                ProcessError(exception);
                if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
            }

            return foundEntity;
        }
        #endregion Public Methods Find

        #region Public Methods Update
        /// <summary>'Cover' method that creates a 'DbContext' to pass to an overloaded 'UpdateEntity' method (creates 'EfCfBaseDbContext' for you).</summary>
        /// <typeparam name="TEntity">POCO class type (representing a db table) in a typed 'DbContext.DbSet' property (e.g. 'public virtual DbSet&lt;Book&gt; Book { get; set; }').</typeparam>
        /// <param name="existingEntity">POCO class instance containing values for an existing entity/record.</param>
        /// <param name="updateEntity">POCO class instance containing values to be used to update an existing entity/record.</param>
        /// <param name="efCfBaseDbContext">EfCfBaseDbContext (DbContext derived class) used to update entity/record in db.</param>
        public void UpdateEntity<TEntity>(TEntity existingEntity, TEntity updateEntity, EfCfBaseDbContext efCfBaseDbContext = null) where TEntity : class
        {
            try
            {
                if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

                //var dbSet = efCfBaseDbContext.GetDbSetByPoco(existingEntity); // Get DbSet by class type.
                var dbSet = efCfBaseDbContext.Set<TEntity>();

                dbSet.Attach(existingEntity); // 'Attach' so the DbContext is 'aware' of the entity; can't update entity if it doesn't think it 'exists' in the first place.

                // Set the values of the 'existing' record to values specified in the 'update' record (note: update statement generated will only include values that are different (for efficiency)).
                efCfBaseDbContext.Entry(existingEntity).CurrentValues.SetValues(updateEntity);

                efCfBaseDbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                ProcessError(exception);
                if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
            }
        }
        #endregion Public Methods Update

        #region Public Methods Delete
        /// <summary>Method to delete an existing entity/record specified by values (i.e. primary key) in the 'deleteEntity' argument instance.</summary>
        /// <typeparam name="TEntity">POCO class type (representing a db table) in a typed 'DbContext.DbSet' property (e.g. 'public virtual DbSet&lt;Book&gt; Book { get; set; }').</typeparam>
        /// <param name="deleteEntity">POCO class instance containing values to find (by primary key) and delete an existing entity/record.</param>
        /// <param name="efCfBaseDbContext">EfCfBaseDbContext (DbContext derived class) used to update entity/record in db.</param>
        public void DeleteEntity<TEntity>(TEntity deleteEntity, EfCfBaseDbContext efCfBaseDbContext = null) where TEntity : class
        {
            try
            {
                if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

                //var dbSet = efCfBaseDbContext.GetDbSetByPoco(deleteEntity); // Get DbSet by class type.
                var dbSet = efCfBaseDbContext.Set<TEntity>() ; // Get DbSet by class type.

                dbSet.Attach(deleteEntity); // 'Attach' so the DbContext is 'aware' of the entity; can't delete entity if it doesn't think it 'exists' in the first place.
                dbSet.Remove(deleteEntity);

                efCfBaseDbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                ProcessError(exception);
                if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
            }
        }
        #endregion Public Methods Delete
        #endregion Public CRUD Methods (Create, Read, Update, Delete)

        #region Public Methods
        #region Public Method GetFirstEntity
        public TEntity GetFirstEntity<TEntity>(TEntity firstEntity, EfCfBaseDbContext efCfBaseDbContext = null) where TEntity : class
        {
            var foundFirstEntity = GetFirstEntity<TEntity>(efCfBaseDbContext);

            return foundFirstEntity;
        }

        public TEntity GetFirstEntity<TEntity>(EfCfBaseDbContext efCfBaseDbContext = null) where TEntity : class
        {
            var firstEntity = default(TEntity);

            try
            {
                if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

                //var dbSet = efCfBaseDbContext.GetDbSetByPoco(firstEntity); // Get DbSet by class type.
                var dbSet = efCfBaseDbContext.Set<TEntity>(); // Get DbSet by class type.

                firstEntity = dbSet.FirstOrDefault();
            }
            catch (Exception exception)
            {
                ProcessError(exception);
                if (this.IsThrowErrorsEnabled) { throw; }  // Throw the error (if enabled).
            }

            return firstEntity;
        }
        #endregion Public Method GetFirstEntity
        
        #region Public Method QueryEntity
        public List<TEntity> QueryEntity<TEntity>(TEntity entity, string sqlQuery, object[] sqlQueryParams = null, EfCfBaseDbContext efCfBaseDbContext = null, bool useAsNoTracking = true) where TEntity : class
        {
            var entityList = QueryEntity<TEntity>(sqlQuery, sqlQueryParams, efCfBaseDbContext, useAsNoTracking);

            return entityList;
        }

        /// <summary>Executes sql against a specific entity (i.e. DbSet for a table). (Note: use 'QueryRawSql' method if sql not related to entity).</summary>
        /// <typeparam name="TEntity">POCO class type (representing a db table).</typeparam>
        /// <param name="entity">POCO class type (representing a db table).</param>
        /// <param name="sqlQuery">Sql statement to be executed.</param>
        /// <param name="sqlQueryParams">Parameters to specify/replace 'value placeholders' in the sql statement.</param>
        /// <param name="efCfBaseDbContext">EfCfBaseDbContext (DbContext derived class) used to query db.</param>
        /// <param name="useAsNoTracking">Flag (true/false) indicating whether entities returned by sql query should be tracked by entity framework (default is 'no tracking').</param>
        /// <returns>A list of POCO classes (representing record(s) in a db table).</returns>
        //public List<TEntity> QueryEntity<TEntity>(TEntity entity, string sqlQuery, object[] sqlQueryParams = null, EfCfBaseDbContext efCfBaseDbContext = null, bool useAsNoTracking = true) where TEntity : class
        //{
        //    // Example: "Select @FieldName1, @FieldName2 From Book Where @FieldName1 = @FieldValue1 And @FieldName2 > @FieldValue2"
        //    var entityList = new List<TEntity>();
        //    try
        //    {
        //        if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.
        //        efCfBaseDbContext.Database.CommandTimeout = 180;
        //        //var dbSet = efCfBaseDbContext.GetDbSetByPoco(entity); // Get DbSet by class type.
        //        var dbSet = efCfBaseDbContext.Set<TEntity>(); // Get DbSet by class type.

        //        var dbQuery = (sqlQueryParams == null) ? dbSet.SqlQuery(sqlQuery) : dbSet.SqlQuery(sqlQuery, sqlQueryParams); // Specify query (using query parameters, if specified).

        //        // Perform query. Query with 'AsNoTracking' so the DbContext won't track changes to the retrieved entities, or not, based on the specified parameter.
        //        entityList = useAsNoTracking ? dbQuery.AsNoTracking().ToList() : dbQuery.ToList();
        //    }
        //    catch (Exception exception)
        //    {
        //        ProcessError(exception);
        //        if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
        //    }

        //    return entityList;
        //}

        public List<TEntity> QueryEntity<TEntity>(string sqlQuery, object[] sqlQueryParams = null, EfCfBaseDbContext efCfBaseDbContext = null, bool useAsNoTracking = true) where TEntity : class
        {
            // Example: "Select @FieldName1, @FieldName2 From Book Where @FieldName1 = @FieldValue1 And @FieldName2 > @FieldValue2"

            var sqlParameterCollectionErrorCount = 0;               // 'SqlParamCollection' error count (part of kludgy workaround for known issue (param collection not cleared when ExecutionStrategy retries)).
            sqlParameterCollectionWorkAroundRetry: var entityList = new List<TEntity>(); // Terrible label is part of the kludgy workaround for known issue (param collection not cleared when ExecutionStrategy retries).

            try
            {
                if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

                //var dbSet = efCfBaseDbContext.GetDbSetByPoco(entity); // Get DbSet by class type.
                var dbSet = efCfBaseDbContext.Set<TEntity>(); // Get DbSet by class type.

                var dbQuery = (sqlQueryParams == null) ? dbSet.SqlQuery(sqlQuery) : dbSet.SqlQuery(sqlQuery, sqlQueryParams); // Specify query (using query parameters, if specified).

                // Perform query. Query with 'AsNoTracking' so the DbContext won't track changes to the retrieved entities, or not, based on the specified parameter.
                entityList = useAsNoTracking ? dbQuery.AsNoTracking().ToList() : dbQuery.ToList();
            }
            #region TO BE REMOVED (when EF known issue with 'Queries using Sql Parameter(s) and retries' is fixed (hopefully EF 6.2)
            // Incredibly kludgy exception workaround/retry code for known EF issue until the EF code base clears the SqlParameterCollection used when ExecutionStrategy retry logic occurs.
            // NOTE: TO BE REPLACED IMMEDIATELY WHEN EF 6.2 BECOMES AVAILABLE (or earliest version when issue is fixed)!!!!!!!!!
            //      (Reference: https://entityframework.codeplex.com/SourceControl/changeset/107283972666babbff10fb7409298f39200acb09 AND http://entityframework.codeplex.com/workitem/2952 ).
            catch (ArgumentException argumentException)
            {
                // Is the ArgumentException the known issue (param collection not cleared when ExecutionStrategy retries)?
                // If so, attempt a 'retry' by rerunning the logic of this method ('jumping' to 'sqlParameterCollectionWorkAroundRetry' label).
                var maxRetryCount = this.EfCfExecutionStrategy.MaxRetryCount;
                if (IsSqlParamAlreadyInSqlParameterCollectionError(argumentException, ref sqlParameterCollectionErrorCount, maxRetryCount)) // If known EF issue, perform kludgy retry until issue is resolved.
                {
                    goto sqlParameterCollectionWorkAroundRetry;
                }
                else
                {
                    ProcessError(argumentException); // If issue is NOT the known EF issue, process error normally.
                    if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
                }
            }
            #endregion TO BE REMOVED (when EF known issue with 'Queries using Sql Parameter(s) and retries' is fixed (hopefully EF 6.2)
            catch (Exception exception)
            {
                ProcessError(exception);
                if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
            }

            return entityList;
        }

        //public List<TEntity> ExecStoredProcedureEntity<TEntity>(TEntity entity, string storedProcName, SqlParameter[] storedProcParams = null, EfCfBaseDbContext efCfBaseDbContext = null) where TEntity : class
        //{
        //    var entityList = ExecStoredProcedureEntity<TEntity>(storedProcName, storedProcParams, efCfBaseDbContext);

        //    return entityList;
        //}

        //public List<TEntity> ExecStoredProcedureEntity<TEntity>(string storedProcName, SqlParameter[] storedProcParams = null, EfCfBaseDbContext efCfBaseDbContext = null) where TEntity : class
        //{
        //    // Example: "exec dbo.Multiply

        //    // ToDo:
        //    //   Add method parameter that is an out parameter of 'out List<object> outParameterValues'
        //    //   Would need a 'mapper' to map SqlDbType to C# type (e.g. 'nvarchar' to 'string') - this would be needed to case 'out' parameters returned from database?
        //    //   Create a stored proc w/ out params to test/research.

        //    var entityList = new List<TEntity>();
        //    try
        //    {
        //        if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

        //        //var dbSet = efCfBaseDbContext.GetDbSetByPoco(entity); // Get DbSet by class type.
        //        var dbSet = efCfBaseDbContext.Set<TEntity>(); // Get DbSet by class type.

        //        object[] storedProcSqlParameters = null;
        //        if (storedProcParams != null)
        //        {
        //            storedProcSqlParameters = new object[] { storedProcParams.ToArray() };  // Convert SqlParameter array to object array for DbSet.SqlQuery parameter.
        //        }

        //        // Specify query (using query parameters, if specified).
        //        var dbQuery = (storedProcSqlParameters == null) ? dbSet.SqlQuery(storedProcName) : dbSet.SqlQuery(storedProcName, storedProcSqlParameters);

        //        entityList = dbQuery.AsNoTracking().ToList();   // Perform query. NOTE: 'AsNoTracking' specified so the DbContext won't track changes to retrieved entities.
        //    }
        //    catch (Exception exception)
        //    {
        //        ProcessError(exception);
        //        if (this.IsThrowErrorsEnabled) { throw exception; }  // Throw the error (if enabled).
        //    }

        //    return entityList;
        //}

        /// <summary>Pass through method to query repository and attempt to map results to specified POCO class.</summary>
        /// <typeparam name="TEntity">POCO class type (to map query results to).</typeparam>
        /// <param name="sqlQuery">Sql statement to be executed.</param>
        /// <param name="sqlQueryParams">Parameters to specify/replace 'value placeholders' in the sql statement.</param>
        /// <param name="efCfBaseDbContext">EfCfBaseDbContext (DbContext derived class) used to query db.</param>
        /// <returns>List of specified POCO class types (mapped to query result values).</returns>
        //public List<TEntity> QueryRawSql<TEntity>(string sqlQuery, object[] sqlQueryParams = null, EfCfBaseDbContext efCfBaseDbContext = null)
        //{
        //    // Example: "Select @FieldName1, @FieldName2 From Book Where @FieldName1 = @FieldValue1 And @FieldName2 > @FieldValue2"
        //    var entityList = new List<TEntity>();
        //    try
        //    {
        //        if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

        //        if (sqlQueryParams == null)
        //        {
        //            entityList = efCfBaseDbContext.Database.SqlQuery<TEntity>(sqlQuery).ToList();
        //        }
        //        else
        //        {
        //            entityList = efCfBaseDbContext.Database.SqlQuery<TEntity>(sqlQuery, sqlQueryParams).ToList();
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        ProcessError(exception);
        //        if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
        //    }

        //    return entityList;
        //}

        public List<TEntity> QueryRawSql<TEntity>(string sqlQuery, object[] sqlQueryParams = null, EfCfBaseDbContext efCfBaseDbContext = null)
        {
            // Example: "Select @FieldName1, @FieldName2 From Book Where @FieldName1 = @FieldValue1 And @FieldName2 > @FieldValue2"

            var sqlParameterCollectionErrorCount = 0;               // 'SqlParamCollection' error count (part of kludgy workaround for known issue (param collection not cleared when ExecutionStrategy retries)).
            sqlParameterCollectionWorkAroundRetry: var entityList = new List<TEntity>(); // Terrible label is part of the kludgy workaround for known issue (param collection not cleared when ExecutionStrategy retries).

            try
            {
                if (efCfBaseDbContext == null)
                {
                    efCfBaseDbContext = this.EfCfBaseDbContext;
                } // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

                if (sqlQueryParams == null)
                {
                    entityList = efCfBaseDbContext.Database.SqlQuery<TEntity>(sqlQuery).ToList();
                }
                else
                {
                    entityList = efCfBaseDbContext.Database.SqlQuery<TEntity>(sqlQuery, sqlQueryParams).ToList();
                }
            }
            #region TO BE REMOVED (when EF known issue with 'Queries using Sql Parameter(s) and retries' is fixed (hopefully EF 6.2)
            // Incredibly kludgy exception workaround/retry code for known EF issue until the EF code base clears the SqlParameterCollection used when ExecutionStrategy retry logic occurs.
            // NOTE: TO BE REPLACED IMMEDIATELY WHEN EF 6.2 BECOMES AVAILABLE (or earliest version when issue is fixed)!!!!!!!!!
            //      (Reference: https://entityframework.codeplex.com/SourceControl/changeset/107283972666babbff10fb7409298f39200acb09 AND http://entityframework.codeplex.com/workitem/2952 ).
            catch (ArgumentException argumentException)
            {
                // Is the ArgumentException the known issue (param collection not cleared when ExecutionStrategy retries)?
                // If so, attempt a 'retry' by rerunning the logic of this method ('jumping' to 'sqlParameterCollectionWorkAroundRetry' label).
                var maxRetryCount = this.EfCfExecutionStrategy.MaxRetryCount;
                if (IsSqlParamAlreadyInSqlParameterCollectionError(argumentException, ref sqlParameterCollectionErrorCount, maxRetryCount)) // If known EF issue, perform kludgy retry until issue is resolved.
                {
                    goto sqlParameterCollectionWorkAroundRetry;
                }
                else
                {
                    ProcessError(argumentException); // If issue is NOT the known EF issue, process error normally.
                    if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
                }
            }
            #endregion TO BE REMOVED (when EF known issue with 'Queries using Sql Parameter(s) and retries' is fixed (hopefully EF 6.2)
            catch (Exception exception)
            {
                ProcessError(exception);
                if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
            }

            return entityList;
        }

        /// <summary>Executes specified sql against context database.</summary>
        /// <param name="sqlQuery">Sql statement to be executed.</param>
        /// <param name="sqlQueryParams">Parameters to specify/replace 'value placeholders' in the sql statement.</param>
        /// <param name="efCfBaseDbContext">EfCfBaseDbContext (DbContext derived class) used to query db.</param>
        /// <returns>Integer count of items affected by executing specified sql (e.g. count of entries deleted).</returns>
        //public int ExecuteSqlCommand(string sqlQuery, object[] sqlQueryParams = null, EfCfBaseDbContext efCfBaseDbContext = null)
        //{
        //    // Example: "Delete From Book Where @FieldName1 = @FieldValue1 And @FieldName2 > @FieldValue2"
        //    var result = 0;
        //    try
        //    {
        //        if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

        //        if (sqlQueryParams == null)
        //        {
        //            result = efCfBaseDbContext.Database.ExecuteSqlCommand(sqlQuery);
        //        }
        //        else
        //        {
        //            result = efCfBaseDbContext.Database.ExecuteSqlCommand(sqlQuery, sqlQueryParams);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        ProcessError(exception);
        //        if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
        //    }

        //    return result;
        //}
        public int ExecuteSqlCommand(string sqlQuery, object[] sqlQueryParams = null, EfCfBaseDbContext efCfBaseDbContext = null)
        {
            // Example: "Delete From Book Where @FieldName1 = @FieldValue1 And @FieldName2 > @FieldValue2"

            var sqlParameterCollectionErrorCount = 0;               // 'SqlParamCollection' error count (part of kludgy workaround for known issue (param collection not cleared when ExecutionStrategy retries)).
            sqlParameterCollectionWorkAroundRetry: var result = 0; // Terrible label is part of the kludgy workaround for known issue (param collection not cleared when ExecutionStrategy retries).

            try
            {
                if (efCfBaseDbContext == null) { efCfBaseDbContext = this.EfCfBaseDbContext; }  // If no EfCfBaseDbContext (DbContext) specified, use EfCfBaseDbContext created during class construction.

                if (sqlQueryParams == null)
                {
                    result = efCfBaseDbContext.Database.ExecuteSqlCommand(sqlQuery);
                }
                else
                {
                    result = efCfBaseDbContext.Database.ExecuteSqlCommand(sqlQuery, sqlQueryParams);
                }
            }
            #region TO BE REMOVED (when EF known issue with 'Queries using Sql Parameter(s) and retries' is fixed (hopefully EF 6.2)
            // Incredibly kludgy exception workaround/retry code for known EF issue until the EF code base clears the SqlParameterCollection used when ExecutionStrategy retry logic occurs.
            // NOTE: TO BE REPLACED IMMEDIATELY WHEN EF 6.2 BECOMES AVAILABLE (or earliest version when issue is fixed)!!!!!!!!!
            //      (Reference: https://entityframework.codeplex.com/SourceControl/changeset/107283972666babbff10fb7409298f39200acb09 AND http://entityframework.codeplex.com/workitem/2952 ).
            catch (ArgumentException argumentException)
            {
                // Is the ArgumentException the known issue (param collection not cleared when ExecutionStrategy retries)?
                // If so, attempt a 'retry' by rerunning the logic of this method ('jumping' to 'sqlParameterCollectionWorkAroundRetry' label).
                var maxRetryCount = this.EfCfExecutionStrategy.MaxRetryCount;
                if (IsSqlParamAlreadyInSqlParameterCollectionError(argumentException, ref sqlParameterCollectionErrorCount, maxRetryCount)) // If known EF issue, perform kludgy retry until issue is resolved.
                {
                    goto sqlParameterCollectionWorkAroundRetry;
                }
                else
                {
                    ProcessError(argumentException); // If issue is NOT the known EF issue, process error normally.
                    if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
                }
            }
            #endregion TO BE REMOVED (when EF known issue with 'Queries using Sql Parameter(s) and retries' is fixed (hopefully EF 6.2)
            catch (Exception exception)
            {
                ProcessError(exception);
                if (this.IsThrowErrorsEnabled) { throw; }  // Rethrow the error (if enabled).
            }

            return result;
        }
        #endregion Public Method QueryEntity

        /// <summary>Runs code that contains a set of operations to be run as a transaction (temporarily suspends custom 'ShouldRetryOn' ExecutionStrategy logic).</summary>
        /// <param name="transactionalOperation">Action/Delegate that contains a set of operations to be run as a transaction (and retried as a set of operations until retry limit is exceeded).</param>
        /// <param name="isLocalThrowErrorEnabled">Flag (true/false) that will cause an error to be 're' thrown.</param> 
        public void RunTransactionalOperations(Action transactionalOperation, bool isLocalThrowErrorEnabled = false)
        {
            try
            {
                var efCfExecutionStrategy = this.EfCfExecutionStrategy; // Determine 'ExecutionStrategy' to use (e.g. 'MsSqlServerEfCfExecutionStrategy', 'MySqlEfCfExecutionStrategy', etc.).

                EfCfDbConfiguration.IsExecutionStrategySuspended = true; // Set flag to 'suspend' custom execution strategy (e.g. including general 'ShouldRetryOn' logic) temporarily.
                
                efCfExecutionStrategy.Execute(transactionalOperation); // Specify strategy 'Execute' method so that 'retry' applies to all sql statements in the transaction.
            }
            catch (Exception exception)
            {
                if (this.Logger != null) { this.Logger.Info("Error: Transaction should/will be rolled back (no commit due to exception)."); } // RetryLimitExceededException, InvalidOperationException (potential errors).

                ProcessError(exception);
                if (this.IsThrowErrorsEnabled | isLocalThrowErrorEnabled) { throw; } // Rethrow the error (if enabled).
            }
            finally
            {
                EfCfDbConfiguration.IsExecutionStrategySuspended = false;  // Set flag to 'enable' custom execution strategy (e.g. including general 'ShouldRetryOn' logic) again.                
            }
        }

        /// <summary>Attempts to create a 'deep clone' of a class using serialization (for a class that includes the '[Serializable]' attribute).</summary>
        /// <typeparam name="TEntity">POCO class type that supports serialization (i.e includes the '[Serializable]' attribute): usually represents a db table.</typeparam>
        /// <param name="entity">POCO class that supports serialization (i.e includes the '[Serializable]' attribute): usually represents a db table.</param>
        /// <returns>A 'deep clone/copy' of the specified class.</returns>
        public TEntity DeepCloneEntityViaSerialization<TEntity>(TEntity entity) where TEntity : class
        {
            TEntity deepCloneEntity = null;

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();      // Serialize 'entity' to MemoryStream.
                    formatter.Serialize(memoryStream, entity);

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    deepCloneEntity = formatter.Deserialize(memoryStream) as TEntity;   // Deserialize 'entity' to cloned, new entity, from MemoryStream.
                }
            }
            catch (Exception exception)
            {
                var logMsg = string.Format("A serialization or deserialization error occuring as part of an attempt to 'deep clone' class:'{0}'.", entity.GetType().Name);

                if (this.Logger != null) { this.Logger.Error(logMsg, exception); }

                if (this.IsThrowErrorsEnabled) { throw; } // Rethrow the error (if enabled).
            }
            return deepCloneEntity;
        }

        /// <summary>Attempts to create a 'deep clone' of a class using json serialization.</summary>
        /// <typeparam name="TEntity">POCO class type to be cloned (usually represents a db table).</typeparam>
        /// <param name="entity">POCO class type to be cloned (usually represents a db table).</param>
        /// <param name="jsonSerializerSettings">Specifies JsonSerializerSettings to be used during serialization and deserialization (default is null (i.e. not specified)).</param>
        /// <returns>A 'deep clone/copy' of the specified class.</returns>
        public TEntity DeepCloneViaJsonSerialization<TEntity>(TEntity entity, JsonSerializerSettings jsonSerializerSettings = null) where TEntity : class
        {
            var clonedEntity = default(TEntity);

            try
            {
                var serializedEntity = (jsonSerializerSettings == null) ? JsonConvert.SerializeObject(entity) : JsonConvert.SerializeObject(entity, jsonSerializerSettings);

                clonedEntity = (jsonSerializerSettings == null) ? JsonConvert.DeserializeObject<TEntity>(serializedEntity) : JsonConvert.DeserializeObject<TEntity>(serializedEntity, jsonSerializerSettings);
            }
            catch (Exception exception)
            {
                var logMsg = string.Format("A serialization or deserialization error occuring as part of an attempt to 'deep clone' class via JsonConvert:'{0}'.", entity.GetType().Name);

                if (this.Logger != null) { this.Logger.Error(logMsg, exception); }

                if (this.IsThrowErrorsEnabled) { throw; } // Rethrow the error (if enabled).
            }

            return clonedEntity;
        }

        /// <summary>Create parameter (DbParameter inherited) based on the currently configured database 'type' (e.g. 'MS Sql Server', 'MariaDb', 'MySql', 'SQLite').</summary>
        /// <param name="parameterName">Name used when creating parameter (e.g. "@personId").</param>
        /// <param name="value">Value used when creating parameter.</param>
        /// <returns>Parameter (DbParameter inherited) based on the currently configured database 'type' (e.g. 'MS Sql Server', 'MariaDb', 'MySql', 'SQLite').</returns>
        public object CreateDbParameter(string parameterName, object value)
        {
            object dbParameter = null;

            switch (this.DbConfigurationDatabaseType)
            {
                case ConfigurationUtility.DbConfigurationDatabaseType.MsSqlServer:
                    dbParameter = new SqlParameter(parameterName, value);
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.MySql:
                    //dbParameter = new MySqlParameter(parameterName, value);
                    var dbParameter1 = new MySqlParameter();
                    dbParameter1.ParameterName = parameterName;
                    dbParameter1.Value = value;

                    dbParameter = dbParameter1;
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.MariaDb:
                    //dbParameter = new MySqlParameter(parameterName, value);
                    var dbParameter2 = new MySqlParameter();
                    dbParameter2.ParameterName = parameterName;
                    dbParameter2.Value = value;

                    dbParameter = dbParameter2;
                    break;

                case ConfigurationUtility.DbConfigurationDatabaseType.Sqlite:
                    dbParameter = new SQLiteParameter(parameterName, value);
                    break;
            }

            return dbParameter;
        }
        #endregion Public Methods

        #region Private Helper Methods
        /// <summary>Provides consolidated error handling, processing and logging.</summary>
        /// <param name="exception">Exception to be evaluated and logged.</param>
        private void ProcessError(Exception exception)
        {
            try
            {
                var validationErrorListText = string.Empty;
                
                var dbEntityValidationException = exception as DbEntityValidationException;
                if (dbEntityValidationException != null && dbEntityValidationException.EntityValidationErrors != null)
                {
                    // Record info about each 'ValidationError' found within each 'EntityValidationError'.
                    var validationErrorList = dbEntityValidationException.EntityValidationErrors.SelectMany(entityValidationError => entityValidationError.ValidationErrors).ToList();

                    var validationErrorListBuilder = new StringBuilder();
                    validationErrorList.ForEach(validationError => validationErrorListBuilder.AppendFormat("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage));
                    validationErrorListText = validationErrorListBuilder.ToString();

                    Trace.TraceInformation(validationErrorListText); // Trace
                }
                else
                {
                    // Some additional, potential errors using Entity Framework:
                    //      InvalidOperationException, 
                    //      EntityException (Inner exception contains the most relevant information (e.g database server out of available connections)).
                    //      DbUpdateException (Not obvious, but the 'Inner Exception' of the 'Inner Exception' contains the relevant information for this exception).
                    //      TransactionAbortedException
                    //      ApplicationException

                    var logMsg = string.Format("{0}.", exception);    // Exception class name, stack trace, and all nested InnerExceptions (if any).

                    if (this.Logger == null) { Trace.TraceError(logMsg); }
                }

                if (this.Logger != null) // Log error info (if logger specified at construction).
                {
                    // Log general exception.
                    this.Logger.Error("Error", exception);

                    // If validation error information was created, log for easier error analysis/review.
                    if (string.IsNullOrEmpty(validationErrorListText) == false)
                    {
                        this.Logger.Info(validationErrorListText);
                    }
                }
            }
            catch (Exception e)
            {
                // Currently just catching exception to suppress any errors in this method, but write to Trace. 
                // ToDo: Still determining what best to do if error occurs here (which could be a logging error/issue).
                var logMsg = string.Format("{0}", e);
                Trace.TraceError(logMsg);
            }
        }

        /// <summary>Enables or disables sql logging of the DbContext operations (by entity framework).</summary>
        private void SetDbContextSqlLogging()
        {
            // Send the SQL generated by Entity Framework to the 'log information' method (if 'verbose' is enabled).
            if (this.IsSqlLoggingEnabled && this.Logger != null)
            {
                this.EfCfBaseDbContext.Database.Log = this.Logger.Info;
            }
        }

        /// <summary>Determine if </summary>
        private void ConfirmDbContextDbProviderSameAsSpecifiedConfigDbProvider()
        {
            var connectionStringName = this.EfCfBaseDbContext.ConnectionStringName;
            var dbTypeFromConfiguration = this.DbConfigurationDatabaseType;

            // Does the 'DbConfigurationDatabaseType' specified in config AppSettings (e.g. '<add key="entityFrameworkFriendlyProviderName" value="MsSqlServer" />') match the 
            // 'DbConfigurationDatabaseType' found when DbContext is actually created (e.g. 'MsSqlServer' was specified in AppSettings, but 'MySql' is specified in connection string)?
            var doesDatabaseProviderMatchConfiguration = ConfigurationUtility.IsDatabaseProviderSameInConfiguration(connectionStringName, dbTypeFromConfiguration);

            if (doesDatabaseProviderMatchConfiguration == false) // If db 'types' don't match, throw custom error to help user understand and fix mismatch.
            {
                var dbTypeFromDbContextConnectionString = ConfigurationUtility.GetDbConfigurationTypeByConnectionStringName(connectionStringName, this.Logger);

                var dbTypeFromConfigurationLabel = dbTypeFromConfiguration.ToString();
                var dbTypeFromDbContextLabel = dbTypeFromDbContextConnectionString.ToString();

                var dbConfigurationDatabaseTypeNameList = ConfigurationUtility.GetDbConfigurationDatabaseTypeValidNameList();
                var supportedDbTypesForConfigurationLabel = string.Join(" | ", dbConfigurationDatabaseTypeNameList);

                var exceptionMessage = string.Format(@"Database provider configuration mismatch:{0}" +
                                                     "the <add key=\"{1}\" value=\"{2}\" /> element under <appSettings> does not match{0}" +
                                                     "the connection string 'name=\"{3}\"' (with db provider type \"{4}\").{0}" +
                                                     "Please modify the appSettings value to match the database provider type.{0}" +
                                                     "Valid values for the 'value=\"\"' attribute (<add key=\"{1}\" value=\"\" />) are: [{5}].",
                                                     Environment.NewLine, Constants.ENTITY_FRAMEWORK_FRIENDLY_PROVIDER_NAME_KEY, dbTypeFromConfigurationLabel, connectionStringName, dbTypeFromDbContextLabel, supportedDbTypesForConfigurationLabel);

                throw new DbProviderTypeMismatchException(exceptionMessage, dbTypeFromDbContextLabel, dbTypeFromConfigurationLabel);
            }
        }

        #region TO BE REMOVED (when EF known issue with 'Queries using Sql Parameter(s) and retries' is fixed (hopefully EF 6.2)
        private bool IsSqlParamAlreadyInSqlParameterCollectionError(ArgumentException argumentException, ref int sqlParameterCollectionErrorCount, int maxRetryCount)
        {
            // Incredibly kludgy exception workaround/retry code for known EF issue until the EF code base clears the SqlParameterCollection used when ExecutionStrategy retry logic occurs.
            // NOTE: TO BE REPLACED IMMEDIATELY WHEN EF 6.2 BECOMES AVAILABLE (or earliest version when issue is fixed)!!!!!!!!!
            //      (Reference: https://entityframework.codeplex.com/SourceControl/changeset/107283972666babbff10fb7409298f39200acb09 AND http://entityframework.codeplex.com/workitem/2952 ).

            const string SQL_PARAM_CONTAINED_IN_PARAM_COLLECTION = "The SqlParameter is already contained by another SqlParameterCollection";

            var isSqlParamAlreadyInSqlParameterCollectionError = false;

            // Is the ArgumentException the known issue (param collection not cleared when ExecutionStrategy retries)?
            var argumentExceptionMessage = argumentException.Message;
            if (argumentExceptionMessage.Contains(SQL_PARAM_CONTAINED_IN_PARAM_COLLECTION))
            {
                isSqlParamAlreadyInSqlParameterCollectionError = true;

                sqlParameterCollectionErrorCount = sqlParameterCollectionErrorCount + 1;

                // (Note: Default 'maxRetry' for ExecutionStrategy = 5 (part of kludgy workaround for known issue (param collection not cleared when ExecutionStrategy retries)).
                // If 'retry limit' (currently 5, same as default EF retry number) is exceeded, throw EF 'RetryLimitExceededException' exception; 
                //      otherwise, wait 'x' second(s) and set 'isSqlParamAlreadyInSqlParameterCollectionError' so retry can occur.
                if (sqlParameterCollectionErrorCount > maxRetryCount)
                {
                    var retryLimitExceededExceptionMessage = string.Format("Exceeded max retry count of {0}.", maxRetryCount);
                    var retryLimitExceededException = new RetryLimitExceededException(retryLimitExceededExceptionMessage);
                    throw retryLimitExceededException;
                }
                else
                {
                    var secondsToWaitBetweenRetries = TimeSpan.FromSeconds(Math.Pow(sqlParameterCollectionErrorCount, 2)); // 'Exponential wait' between retries; e.g. wait 1^2=1, 2^2=4, 3^2=9, 4^2=16, 5^2=25 (seconds).
                    Thread.Sleep(secondsToWaitBetweenRetries);

                    if (this.Logger != null) { this.Logger.InfoFormat("EF SqlParameterCollection error retry {0} of {1}; retrying in {2} second(s).", sqlParameterCollectionErrorCount, maxRetryCount, secondsToWaitBetweenRetries); }
                }
            }

            return isSqlParamAlreadyInSqlParameterCollectionError;
        }
        #endregion TO BE REMOVED (when EF known issue with 'Queries using Sql Parameter(s) and retries' is fixed (hopefully EF 6.2)
        #endregion Private Helper Methods
    }
}
