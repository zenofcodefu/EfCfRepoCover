using System;
using EfCfRepoCoverLib;
using EfCfRepoCoverTests.Repository.EfCodeFirstLibDb.Entities;
using Logging.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace EfCfRepoCoverTests.Repository.EfCodeFirstLibDb
{
    public class EfCodeFirstLibRepository : EfCfBaseRepository
    {
        private EfCodeFirstLibDbContext EfCodeFirstLibDbContext { get; set; }

        private ILogging Logger { get; set; }

        #region Constructors
        public EfCodeFirstLibRepository(bool isThrowErrorsEnabled = true, ILogging logger = null, bool isLoggingVerbose = false, bool isSqlLoggingEnabled = false)
            : base(new EfCodeFirstLibDbContext(), isThrowErrorsEnabled, logger, isLoggingVerbose, isSqlLoggingEnabled)
        {
            this.EfCodeFirstLibDbContext = (EfCodeFirstLibDbContext)this.EfCfBaseDbContext;
            this.Logger = logger;
        }

        public EfCodeFirstLibRepository(string connectionStringName, bool isThrowErrorsEnabled = true, ILogging logger = null, bool isLoggingVerbose = false, bool isSqlLoggingEnabled = false)
            : base(new EfCodeFirstLibDbContext(connectionStringName), isThrowErrorsEnabled, logger, isLoggingVerbose, isSqlLoggingEnabled)
        {
            this.EfCodeFirstLibDbContext = (EfCodeFirstLibDbContext)this.EfCfBaseDbContext;
            this.Logger = logger;
        }
        #endregion Constructors

        #region Public Methods

        public Person PersonCreate(Person person)
        {
            var addedPerson = base.AddEntity(person);

            return addedPerson;
        }

        public Person PersonRead(object[] keyValues = null)
        {
            var foundPerson = base.FindEntity<Person>(keyValues);

            return foundPerson;
        }

        public void PersonUpdate(Person existingPerson, Person updatePerson)
        {            
            base.UpdateEntity(existingPerson, updatePerson);
        }

        public void PersonDelete(Person person)
        {
            base.DeleteEntity(person);
        }

        public bool AddPersonAndStudentTransaction(Person person, Student student)
        {
            var status = false;

            // Specify 'operations' (e.g. sql inserts, updates, etc.) to be run as a transaction w/ retry attempts (specify a method that represents a transaction).
            try
            {
                base.RunTransactionalOperations(() => status = AddPersonAndStudent(person, student), isLocalThrowErrorEnabled: true);
            }
            catch (Exception exception)
            {
                // Catching rethrown exception to determine 'success/failure'; actual Exception data will be logged in the EfCfBaseRepository.RunTransactionalOperations method.
                if (this.Logger != null)
                {
                    var logMsg = string.Format("Error attempting to add 'Person' and 'Student' in a user-initiated transaction (possible ExecutionStrategy problem?). Error:{0}", exception.ToString());
                    this.Logger.Error(logMsg);
                }
                status = false;
            }

            return status;
        }

        public bool AddPersonAndStudent(Person person, Student student)
        {
            var status = false;

            var efCodeFirstLibRepository = new EfCodeFirstLibRepository(logger:this.Logger);

            using (var dbContextTransaction = efCodeFirstLibRepository.EfCodeFirstLibDbContext.Database.BeginTransaction())
            {
                try
                {
                    var addedPerson = efCodeFirstLibRepository.AddEntity(person);

                    student.PersonId = addedPerson.PersonId;

                    var addedStudent = efCodeFirstLibRepository.AddEntity(student);

                    dbContextTransaction.Commit();

                    status = true;
                }
                catch (Exception exception)
                {
                    dbContextTransaction.Rollback();

                    if (this.Logger != null) { this.Logger.Error("Error", exception); }

                    throw;
                }
            }

            return status;
        }

        public List<Student> GetStudentListByPersonId(int personId)
        {
            var studentList = new List<Student>();

            if (personId < 0) { return studentList; } // If 'personId' is null, no need to continue; 'early return' here.

            const string sqlQuery = @"SELECT StudentId, PersonId, CourseCount
                                      FROM Student
                                      WHERE PersonId = @personId";

            var sqlParamPersonId = this.CreateDbParameter("@personId", personId);

            var sqlQueryParams = new object[] {sqlParamPersonId};

            //studentList = base.QueryEntity(new Student(), sqlQuery, sqlQueryParams).ToList();
            studentList = base.QueryEntity<Student>(sqlQuery, sqlQueryParams).ToList();

            return studentList;
        }

        public Student StudentCreate(Student student)
        {
            var addedStudent = base.AddEntity(student);

            return addedStudent;
        }

        public Student StudentRead(object[] keyValues = null)
        {
            var foundStudent = base.FindEntity<Student>(keyValues);

            return foundStudent;
        }
        #endregion Public Methods
    }
}
