using System;
using System.Data.Entity;
using EfCfRepoCoverLib;
using EfCfRepoCoverExamples.Repository.YourDatabaseNameHere.Entities;

namespace EfCfRepoCoverExamples.Repository.YourDatabaseNameHere
{
    public class YourDatabaseNameDbContext : EfCfBaseDbContext
    {
        #region Constructors
        public YourDatabaseNameDbContext()
        {
            Initialize();
        }

        public YourDatabaseNameDbContext(string connectionStringName)
            : base(connectionStringName)
        {
            Initialize();
        }
        #endregion Constructors

        #region Public Properties
        public virtual DbSet<YourClassRepresentingDbTableHere> Person { get; set; }
        #endregion Public Properties

        private void Initialize()
        {
            Database.SetInitializer<YourDatabaseNameDbContext>(null);    // Initialize the 'SetInitializer' for the DBContext to 'null', so it won't try to create entities (existing database).                        
        }
    }
}
