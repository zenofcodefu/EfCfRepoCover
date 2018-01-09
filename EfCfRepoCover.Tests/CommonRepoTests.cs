using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EfCfRepoCoverTests.Repository.EfCodeFirstLibDb;
using EfCfRepoCoverTests.Repository.EfCodeFirstLibDb.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EfCfRepoCoverTests
{
    public class CommonRepoTests
    {
        private static readonly object ParameterizedQueryLockObject = new object();

        public void CreateTest<T>()
        {
            // Arrange
            const string FAMILY_NAME = "Xavier";
            const string FIRST_NAME = "Charles";
            const int PET_COUNT = 1;

            var person = new Person {FamilyName = FAMILY_NAME, FirstName = FIRST_NAME, PetCount = PET_COUNT }; // Create 'Person' object to be added to database/repository.

            var efCodeFirstLibRepository = new EfCodeFirstLibRepository();

            // Act
            var createdPerson = efCodeFirstLibRepository.PersonCreate(person);

            DeleteEntity(createdPerson); // Cleanup: Attempt to delete here/now in case an assert fails.

            // Assert
            Assert.IsNotNull(createdPerson);
            Assert.IsTrue(createdPerson.PersonId > -1);
            Assert.IsTrue(createdPerson.FamilyName.Equals(FAMILY_NAME));
            Assert.IsTrue(createdPerson.FirstName.Equals(FIRST_NAME));
            Assert.IsTrue(createdPerson.PetCount.Equals(PET_COUNT));
        }

        [TestMethod]
        public void ReadTest<T>()
        {
            // Arrange
            var person = new Person { FamilyName = "Grey", FirstName = "Jean", PetCount = 2 }; // Create 'Person' to 'find/select' for test.

            var efCodeFirstLibRepository = new EfCodeFirstLibRepository();
            var createdPerson = efCodeFirstLibRepository.PersonCreate(person);

            var createdPersonPrimaryKeyValues = new object[] { createdPerson.PersonId }; // Specify 'primary key' to 'find/select' newly created 'Person'.

            // Act
            var foundPerson = efCodeFirstLibRepository.PersonRead(createdPersonPrimaryKeyValues);

            DeleteEntity(createdPerson); // Cleanup: Attempt to delete here/now in case an assert fails.

            // Assert
            Assert.IsNotNull(foundPerson);
            Assert.IsTrue(createdPerson.PersonId.Equals(foundPerson.PersonId));
            Assert.IsTrue(createdPerson.FamilyName.Equals(foundPerson.FamilyName));
            Assert.IsTrue(createdPerson.FirstName.Equals(foundPerson.FirstName));
            Assert.IsTrue(createdPerson.PetCount.Equals(foundPerson.PetCount));
        }

        [TestMethod]
        public void UpdateTest<T>()
        {
            const int PET_COUNT = 3;

            // Arrange
            var person = new Person { FamilyName = "Summers", FirstName = "Scott", PetCount = PET_COUNT }; // Create 'Person' to 'find/select' for test.

            var efCodeFirstLibRepository = new EfCodeFirstLibRepository();
            var createdPerson = efCodeFirstLibRepository.PersonCreate(person);

            var createdPersonPrimaryKeyValues = new object[] { createdPerson.PersonId }; // Specify 'primary key' to 'find/select' newly created 'Person'.
            var existingPerson = efCodeFirstLibRepository.PersonRead(createdPersonPrimaryKeyValues); // 'Find/select' newly created 'Person' to update.

            var updatedValuesPerson = efCodeFirstLibRepository.DeepCloneViaJsonSerialization(existingPerson); // 'Clone' the 'foundPerson' to make update easier (i.e. easier than manual/copy object initialiation).

            const int PET_COUNT_INCREASE = 10; 
            const int EXPECTED_PET_COUNT_TOTAL = PET_COUNT + PET_COUNT_INCREASE;
            updatedValuesPerson.PetCount = updatedValuesPerson.PetCount + PET_COUNT_INCREASE; // 'Updating' the newly created 'Person' to increase the 'PetCount' property (increment by 10).

            // Act
            efCodeFirstLibRepository.PersonUpdate(existingPerson, updatedValuesPerson);

            DeleteEntity(createdPerson); // Cleanup: Attempt to delete here/now in case an assert fails.

            // Assert
            Assert.IsNotNull(updatedValuesPerson);
            Assert.IsTrue(updatedValuesPerson.PetCount.Equals(EXPECTED_PET_COUNT_TOTAL));
        }

        [TestMethod]
        public void DeleteTest<T>()
        {
            // Arrange
            var person = new Person { FamilyName = "Howlett", FirstName = "James", PetCount = 4 }; // Create 'Person' to use in test.

            var efCodeFirstLibRepository = new EfCodeFirstLibRepository();
            var createdPerson = efCodeFirstLibRepository.PersonCreate(person);

            var createdPersonPrimaryKeyValues = new object[] { createdPerson.PersonId }; // Specify 'primary key' to 'find/select' newly created 'Person'.

            // Act
            efCodeFirstLibRepository.PersonDelete(createdPerson); // Attempt to delete newly created 'Person'.

            var foundPerson = efCodeFirstLibRepository.PersonRead(createdPersonPrimaryKeyValues); // Attempt to find deleted 'Person' (null is expected, since newly created 'Person' should be deleted).

            // Assert
            Assert.IsNull(foundPerson);
        }

        [TestMethod]
        public void UserInitiatedTransactionTest<T>()
        {
            // Arrange
            var person = new Person { FamilyName = "McCoy", FirstName = "Henry", PetCount = 5 }; // Create 'Person' to use in test.

            var student = new Student() { CourseCount = 5 };

            var efCodeFirstLibRepository = new EfCodeFirstLibRepository();

            // Act
            var wasUserInitiatedTransactionSuccessful = efCodeFirstLibRepository.AddPersonAndStudentTransaction(person, student);

            var createdPersonPrimaryKeyValues = new object[] { person.PersonId }; // Specify 'primary key' to 'find/select' newly created 'Person'.
            var createdStudentPrimaryKeyValues = new object[] { student.StudentId }; // Specify 'primary key' to 'find/select' newly created 'Student'.

            var foundPerson = efCodeFirstLibRepository.PersonRead(createdPersonPrimaryKeyValues); // Attempt to find created 'Person' (null is expected, since newly created 'Person' should be deleted).
            var foundStudent = efCodeFirstLibRepository.StudentRead(createdStudentPrimaryKeyValues); // Attempt to find created 'Student' (null is expected, since newly created 'Person' should be deleted).

            DeleteEntity(foundStudent); // Cleanup: Attempt to delete here/now in case an assert fails. (Deleting 'Student' first due to 'PersonId' foreign key constraint.)
            DeleteEntity(foundPerson); // Cleanup: Attempt to delete here/now in case an assert fails.

            // Assert
            Assert.IsTrue(wasUserInitiatedTransactionSuccessful);
        }

        [TestMethod]
        public void QueryWithParametersTest<T>()
        {
            // Locking this test code section to avoid puzzling exception with 'MySql': 'Only MySqlParameter objects may be stored'.
            if (Monitor.TryEnter(ParameterizedQueryLockObject))
            {
                try
                {
                    // Arrange
                    var person = new Person { FamilyName = "Wagner", FirstName = "Kurt", PetCount = 6 }; // Create 'Person' to use in test.

                    var student = new Student() { CourseCount = 6 }; // Create 'Student' to use in test.

                    var efCodeFirstLibRepository = new EfCodeFirstLibRepository();

                    var createdPerson = efCodeFirstLibRepository.PersonCreate(person); // Create 'Person' in repository.

                    student.PersonId = createdPerson.PersonId;
                    var createdStudent = efCodeFirstLibRepository.StudentCreate(student); // Create 'Student' in repository.

                    var personId = createdPerson.PersonId;

                    // Act
                    var studentList = efCodeFirstLibRepository.GetStudentListByPersonId(personId);

                    DeleteEntity(createdStudent); // Cleanup: Attempt to delete here/now in case an assert fails. (Deleting 'Student' first due to 'PersonId' foreign key constraint.)
                    DeleteEntity(createdPerson); // Cleanup: Attempt to delete here/now in case an assert fails.

                    // Assert
                    Assert.IsTrue(studentList.Any());
                    Assert.IsTrue(studentList.Count.Equals(1));
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Exception: {0}.", exception.ToString()));
                }
                finally
                {
                    Monitor.Exit(ParameterizedQueryLockObject);
                }
            }
        }

        private void DeleteEntity<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                var efCodeFirstLibRepository = new EfCodeFirstLibRepository();

                efCodeFirstLibRepository.DeleteEntity(entity);
            }
            catch (Exception exception)
            {
                var message = string.Format("Exception occurred attempting to delete 'entity' during a test. Error:{0}.", exception.ToString());
                System.Diagnostics.Debug.WriteLine(message);
            }
        }
    }
}
