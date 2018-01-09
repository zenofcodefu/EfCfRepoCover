using System;
using EfCfRepoCoverLib.CustomErrors;
using EfCfRepoCoverExamples.Repository.EfCodeFirstLibDb;
using EfCfRepoCoverExamples.Repository.EfCodeFirstLibDb.Entities;

namespace EfCfRepoCoverExamples
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                RunCreateReadUpdateDeleteRepoExample(); // Example of basic CRUD operations
                RunUserInitiatedTransactionExample();
                RunQueryWithParametersExample();
            }
            catch (DbProviderTypeMismatchException dbProviderTypeMismatchException)
            {
                // If you're getting this error, make sure the <appSettings> element specified database type (e.g. 'MySql') matches the 'ProviderName=' database type in the connection string being used.
                // (e.g. '<add key="entityFrameworkFriendlyProviderName" value="MsSqlServer" /> but 'ProviderName="System.Data.SQLite"' is specified in connection string)?
                System.Diagnostics.Debug.WriteLine(dbProviderTypeMismatchException.ToString());
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
        }

        private static void RunCreateReadUpdateDeleteRepoExample()
        {
            var personSmith = new Person { FamilyName = "Smith", FirstName = "Pat", PetCount = 1 };                 // Create 'Person' object to be added to database/repository.

            var efCodeFirstLibRepository = new EfCodeFirstLibRepository();

            var createdPersonSmith = efCodeFirstLibRepository.PersonCreate(personSmith);                            // Insert new 'Person' record (from specified object values).

            var foundPersonSmith = efCodeFirstLibRepository.PersonFind(createdPersonSmith);                         // Find newly created 'Person' record (e.g. 'Identity/Primary Key' will have been incremented).

            var updatedPersonSmith = efCodeFirstLibRepository.DeepCloneViaJsonSerialization(foundPersonSmith);      // Modify 'PetCount' property (incrementy by 10).
            updatedPersonSmith.PetCount = updatedPersonSmith.PetCount + 10;

            efCodeFirstLibRepository.PersonUpdate(foundPersonSmith, updatedPersonSmith);                            // Update 'Person' record (with increased 'PetCount').

            efCodeFirstLibRepository.DeleteEntity(foundPersonSmith);                                                // Remove/delete 'Person' record.
        }

        private static void RunUserInitiatedTransactionExample()
        {
            var person = new Person { FamilyName = "Baker", FirstName = "Chris", PetCount = 1 };                    // Create 'Person' object to be added to database/repository.

            var student = new Student() {CourseCount = 5};

            var efCodeFirstLibRepository = new EfCodeFirstLibRepository();

            var wasUserInitiatedTransactionSuccessful = efCodeFirstLibRepository.AddPersonAndStudentTransaction(person, student);
        }

        private static void RunQueryWithParametersExample()
        {
            var person = new Person { FamilyName = "Jones", FirstName = "Sam", PetCount = 3 };

            var student = new Student() { CourseCount = 7 };

            var efCodeFirstLibRepository = new EfCodeFirstLibRepository();

            var createdPerson = efCodeFirstLibRepository.PersonCreate(person);

            var personId = createdPerson.PersonId;

            student.PersonId = personId;
            var createdStudent = efCodeFirstLibRepository.StudentCreate(student);

            var studentList = efCodeFirstLibRepository.GetStudentListByPersonId(personId);
        }
    }
}
