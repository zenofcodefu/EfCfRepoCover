# EfCfRepoCover
Entity Framework (Code First) Repository Cover library (multiple database support (e.g. MS Sql Server, MySql/MariaDb, SQLite), consolidated error handling, user-initiated transactions and database-specific (encapsulated) retry logic, logging injection, and generated sql logging).

**Purpose**: 
    What is the point in creating (or using) a library that essentially just wraps/covers Entity Framework (Code First) CRUD (Create, Read, Update, Delete) methods?

- It's a 'Generic' repository; you can specify any table (poco class) in a database (DbContext), because the methods are generic (no need to write CRUD methods specific to each table (poco class)).
- The code to use this class is small (e.g one line for a CRUD operation (e.g. 'var bookFound = base.FindEntity(Book, keyValues, libraryDbContext)'), with option to 'saveNow' for each operation).
- Supports multiple databases without code changes (e.g. MS SQL Server, MySql/MariaDb, SQLite) with database-specific connection resiliency
- Allows easy use of 'user initiated transactions' with 'connection resiliency' (i.e. 'retries') without additional coding (hides the code involved to allow user-initiated transactions with 'retry' logic).
- Allows 'connection resiliency' (i.e. 'retries') for all operations (including 'user initiated transactions').	
	- (Note: Provides kludgy exception workaround/retry code for known EF issue with the EF code base not clearing the SqlParameterCollection used when ExecutionStrategy retry logic occurs.)
	- (Reference: https://entityframework.codeplex.com/SourceControl/changeset/107283972666babbff10fb7409298f39200acb09 AND http://entityframework.codeplex.com/workitem/2952 ).
	- (Note: THIS TEMP WORKAROUND WILL BE REPLACED IMMEDIATELY WHEN ISSUE IS FIXED IN THE EF CODE BASE)!!!!!!!!!)
- Allows 'single source', injectable (non-static) logging for all operations (including error, retries, user-initiated transactions, and EF created sql (implement the 'ILogging' interface and use whatever logger you prefer).
- Multiple database 'types' supported (e.g. MS Sql Server, SQLite, MySql/MariaDb: any database that supports EntityFramework can be added.)
- Consistent, 'single source' error handling.
	- (Some Entity Framework exceptions/errors can be a little tricky to catch the right error type and retrieve/format the relevant information; errors are caught (formatted if relevant) and logged/traced for you.)
- Provides some handy utility methods for efCf operations (e.g. json (& serializable) 'deep clone' methods to make creating an 'update' entity easier (e.g. one line to 'deep clone' entity, then make 'update' changes to relevant properties)).

## Usage:

### General: Create two classes;
1. Class derived from 'EfCfBaseDbContext' (represents database and contains 'DbSet<>' properties (each property represents a table in the database)).
2. class derived from 'EfCfBaseRepository' (provides access to built-in (inherited) CRUD methods; add any additional methods for interacting with the database here)
      
### Specific:
- Create a C# class for each table you want to work with in a database (e.g. 'public class Book'). (Note: There are EF tools to generate C# classes from existing tables in a database.)
- Create a 'DbContext' derived class (e.g. 'LibraryDbContext : EfCfBaseDbContext').
	- Add to that class, a DbSet<> property for each table in the database you want to work with (e.g. 'public virtual DbSet<Book> Book { get; set; }' - represents 'Book' table in a database).
- Create a 'EfCfBaseRepository' derived class (e.g. 'LibraryRepository : EfCfBaseRepository').
- You're done. Use any desired, existing CRUD methods via the repository (built-in (base) are shown below; add any additional methods you like for working with the tables in this derived class). 
    
```
Example usage of repository CRUD (Create, Read, Update, Delete) methods:
var bookAdded = LibaryRepository.AddEntity(bookToAdd);              // Create
var bookFound = LibaryRepository.FindEntity(bookToFind, keyValues); // Read
LibaryRepository.UpdateEntity(currentBook, updateBook);             // Update
LibaryRepository.DeleteEntity(deleteBook);                          // Delete

QueryRawSql()                 // Sql executed against DbContext database; attempts to map results to specified POCO class
RunTransactionalOperations()  // Run 'user-initiated' transactions with 'DbExecutionStrategy', 'retry' logic
```
