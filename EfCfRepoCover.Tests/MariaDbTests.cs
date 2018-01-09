using System;
using EfCfRepoCoverLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EfCfRepoCoverTests
{
    [TestClass]
    public class MariaDbTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            // Get and set config file here specific to the database type/provider (e.g. 'MS Sql Server', 'MySql', 'MariaDb', 'SQLite', etc.).

            try
            {
                const ConfigurationUtility.DbConfigurationDatabaseType dbConfigurationDatabaseTypeValue = ConfigurationUtility.DbConfigurationDatabaseType.MariaDb;

                var fullyQualifiedConfigFileName = UtilGeneral.GetFullyQualifiedConfigFileNameByDbConfigurationDatabaseType(dbConfigurationDatabaseTypeValue);

                AppConfig.Change(fullyQualifiedConfigFileName);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
        }

        [TestCategory("EfCfLibNet - Provider MariaDb")]
        [TestMethod]
        public void CreateTest_MariaDb()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.CreateTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MariaDb")]
        [TestMethod]
        public void ReadTest_MariaDb()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.ReadTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MariaDb")]
        [TestMethod]
        public void UpdateTest_MariaDb()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.UpdateTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MariaDb")]
        [TestMethod]
        public void DeleteTest_MariaDb()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.DeleteTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MariaDb")]
        [TestMethod]
        public void UserInitiatedTransactionTest_MariaDb()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.UserInitiatedTransactionTest<GenericParameterHelper>();
        }

        [TestCategory("EfCfLibNet - Provider MariaDb")]
        [TestMethod]
        public void QueryWithParametersTest_MariaDb()
        {
            var commonRepoTests = new CommonRepoTests();
            commonRepoTests.QueryWithParametersTest<GenericParameterHelper>();
        }
    }
}
