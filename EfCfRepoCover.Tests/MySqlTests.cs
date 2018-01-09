using System;
using EfCfRepoCoverLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EfCfRepoCoverTests
{
    [TestClass]
    public class MySqlTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            // Get and set config file here specific to the database type/provider (e.g. 'MS Sql Server', 'MySql', 'MariaDb', 'SQLite', etc.).

            try
            {
                const ConfigurationUtility.DbConfigurationDatabaseType dbConfigurationDatabaseTypeValue = ConfigurationUtility.DbConfigurationDatabaseType.MySql;

                var fullyQualifiedConfigFileName = UtilGeneral.GetFullyQualifiedConfigFileNameByDbConfigurationDatabaseType(dbConfigurationDatabaseTypeValue);

                AppConfig.Change(fullyQualifiedConfigFileName);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
        }

        [TestCategory("EfCfLibNet - Provider MySql")]
        [TestMethod]
        public void CreateTest_MySql()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.CreateTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MySql")]
        [TestMethod]
        public void ReadTest_MySql()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.ReadTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MySql")]
        [TestMethod]
        public void UpdateTest_MySql()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.UpdateTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MySql")]
        [TestMethod]
        public void DeleteTest_MySql()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.DeleteTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MySql")]
        [TestMethod]
        public void UserInitiatedTransactionTest_MySql()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.UserInitiatedTransactionTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MySql")]
        [TestMethod]
        public void QueryWithParametersTest_MySql()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.QueryWithParametersTest<GenericParameterHelper>();
        }
    }
}
