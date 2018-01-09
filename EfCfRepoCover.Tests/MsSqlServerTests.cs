using System;
using EfCfRepoCoverLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EfCfRepoCoverTests
{
    [TestClass]
    public class MsSqlServerTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            // Get and set config file here specific to the database type/provider (e.g. 'MS Sql Server', 'MySql', 'MariaDb', 'SQLite', etc.).

            try
            {
                const ConfigurationUtility.DbConfigurationDatabaseType dbConfigurationDatabaseTypeValue = ConfigurationUtility.DbConfigurationDatabaseType.MsSqlServer;

                var fullyQualifiedConfigFileName = UtilGeneral.GetFullyQualifiedConfigFileNameByDbConfigurationDatabaseType(dbConfigurationDatabaseTypeValue);

                AppConfig.Change(fullyQualifiedConfigFileName);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
        }

        [TestCategory("EfCfLibNet - Provider MsSqlServer")]
        [TestMethod]
        public void CreateTest_MsSqlServer()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.CreateTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MsSqlServer")]
        [TestMethod]
        public void ReadTest_MsSqlServer()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.ReadTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MsSqlServer")]
        [TestMethod]
        public void UpdateTest_MsSqlServer()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.UpdateTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MsSqlServer")]
        [TestMethod]
        public void DeleteTest_MsSqlServer()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.DeleteTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MsSqlServer")]
        [TestMethod]
        public void UserInitiatedTransactionTest_MsSqlServer()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.UserInitiatedTransactionTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MsSqlServer")]
        [TestMethod]
        public void QueryWithParametersTest_MsSqlServer()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.QueryWithParametersTest<GenericParameterHelper>();
        }
    }
}
