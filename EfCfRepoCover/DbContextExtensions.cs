using System;
using System.Data.Entity;

namespace EfCfRepoCoverLib
{
    public static class DbContextExtensions
    {
        /// <summary>This method expects to perform reflection upon a child class that inherits from 'DbContext' and contains 'DbSet&lt;TEntity&gt;' properties
        /// (e.g. 'LibraryDbContext' inherits from DbContext' and has 'public virtual DbSet&lt;Book&gt; Book { get; set; }' property.
        /// In this example, 'TDbContext' is 'LibraryDbContext' and 'TEntity' is 'Book').</summary>
        /// <typeparam name="TDbContext">Class that inherits from DbContext that contains typed 'DbContext.DbSet' property(ies) (e.g. 'public virtual DbSet&lt;Book&gt; Book { get; set; }').</typeparam>
        /// <typeparam name="TEntity">POCO class type (representing a db table) in a typed 'DbContext.DbSet' property (e.g. 'public virtual DbSet&lt;Book&gt; Book { get; set; }').</typeparam>
        /// <param name="dbContextInstance">Class that inherits from DbContext that contains typed 'DbContext.DbSet' property(ies) (e.g. 'public virtual DbSet&lt;Book&gt; Book { get; set; }').</param>
        /// <param name="pocoEntity">POCO class type (representing a db table) in a typed 'DbContext.DbSet' property (e.g. 'public virtual DbSet&lt;Book&gt; Book { get; set; }').</param>
        /// <returns>A typed 'DbContext.DbSet' from the DbSet property that matched the 'pocoEntity' type name. (e.g. 'public virtual DbSet&lt;Book&gt; Book { get; set; }').</returns>
        public static DbSet<TEntity> GetDbSetByPoco<TDbContext, TEntity>(this TDbContext dbContextInstance, TEntity pocoEntity)
            where TDbContext : DbContext
            where TEntity : class
        {
            var pocoTypeName = pocoEntity.GetType().Name;                   // POCO (Plain old C# Object, i.e. class) that represents an entity/table (e.g. 'Book').
            var dbContextInstanceName = dbContextInstance.GetType().Name;   // DbContext derived class (e.g. 'LibraryDbContext').

            // Find property in 'DbContext' class that matches the POCO entity/table name (e.g. 'public virtual DbSet<Book> Book { get; set; }').
            var instanceProperty = dbContextInstance.GetType().GetProperty(pocoTypeName);

            // If expected property can't be found, throw error that specifies most likely reason (i.e. no property for 'DbSet<TEntity>' exists in the 'dbContextInstance' object).
            if (instanceProperty == null)
            {
                var errorMsg = string.Format("Expected property name '{0}' was not found for DbContext derived class '{1}' " +
                                             "(e.g. property 'public virtual DbSet<{0}> {0} {{ get; set; }}').", pocoTypeName, dbContextInstanceName);
                throw new Exception(errorMsg);
            }

            // If expected property was found, confirm property 'type' is the expected type (i.e. 'DbSet<TEntity>'); if not throw error with problem description.
            //if (instanceProperty.PropertyType != typeof(DbSet<TEntity>))
            if (instanceProperty.PropertyType.Name.Equals("DbSet`1", StringComparison.OrdinalIgnoreCase) == false)
            {
                var propertyTypeName = instanceProperty.PropertyType.Name;
                var errorMsg = string.Format("Expected property name '{0}' was found for DbContext derived class '{1}' " +
                                             "but expected type of DbSet<{0}> was not found; type '{2}' was found instead.", pocoTypeName, dbContextInstanceName, propertyTypeName);
                throw new Exception(errorMsg);                    
            }

            // Get property in 'DbContext' class that matches the POCO entity/table name (e.g. 'public virtual DbSet<Book> Book { get; set; }').
            var instancePropertyValue = instanceProperty.GetValue(dbContextInstance, null);

            // Cast property to expected return type, 'DbSet<TEntity>' (e.g. 'DbSet<Book').
            var dbSet = instancePropertyValue as DbSet<TEntity>;
            
            return dbSet;
        }
    }
}
