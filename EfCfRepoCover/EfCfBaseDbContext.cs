using System;
using System.Data.Entity;

namespace EfCfRepoCoverLib
{
    //[DbConfigurationType(typeof(EfCfDbConfiguration))]
    public class EfCfBaseDbContext : DbContext
    {
        #region Constructors
        public EfCfBaseDbContext()
        {
            // If the connection string isn't specified, the expected convention for the 'Connection String Name' is the name of the child class derived from this class (i.e. inherited from 'EfCfBaseDbContext').
            //  (e.g. <add name="EfCodeFirstLibDbContext" connectionString="Server=localhost;Database=EfCodeFirstLibDb;Integrated Security=SSPI;MultipleActiveResultSets=True;Max Pool Size=600" providerName="System.Data.SqlClient" />).
            var unspecifiedConnectionStringNameByConvention = this.GetType().Name;

            this.ConnectionStringName = unspecifiedConnectionStringNameByConvention;
        }
        public EfCfBaseDbContext(string connectionStringName) : base(connectionStringName)
        {
            this.ConnectionStringName = connectionStringName;
        }
        #endregion Constructors

        #region Public Properties
        public virtual DbModelBuilder DbModelBuilder { get; private set; }

        public string ConnectionStringName { get; private set; }
        #endregion Public Properties

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            this.DbModelBuilder = modelBuilder;
        }
    }
}
