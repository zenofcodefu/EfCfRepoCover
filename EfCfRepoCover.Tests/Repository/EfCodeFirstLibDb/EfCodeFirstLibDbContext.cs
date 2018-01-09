using System;
using System.Data.Entity;
using EfCfRepoCoverLib;
using EfCfRepoCoverTests.Repository.EfCodeFirstLibDb.Entities;

namespace EfCfRepoCoverTests.Repository.EfCodeFirstLibDb
{
    public class EfCodeFirstLibDbContext : EfCfBaseDbContext
    {
        #region Constructors
        public EfCodeFirstLibDbContext()
        {
            Initialize();
        }

        public EfCodeFirstLibDbContext(string connectionStringName)
            : base(connectionStringName)
        {
            Initialize();
        }
        #endregion Constructors

        #region Public Properties
        public virtual DbSet<Person> Person { get; set; }
        public virtual DbSet<Student> Student { get; set; }
        #endregion Public Properties

        private void Initialize()
        {
            Database.SetInitializer<EfCodeFirstLibDbContext>(null);    // Initialize the 'SetInitializer' for the DBContext to 'null', so it won't try to create entities (existing database).                        
        }
    }
}
