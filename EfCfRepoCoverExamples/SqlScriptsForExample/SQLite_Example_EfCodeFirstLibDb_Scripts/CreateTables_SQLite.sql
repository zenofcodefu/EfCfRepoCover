
PRAGMA foreign_keys = off;
BEGIN TRANSACTION;

/* ***** (Begin) Create Table [Person] ***** */
CREATE TABLE Person (
    PersonId   INTEGER      PRIMARY KEY AUTOINCREMENT,
    FirstName  VARCHAR (50) NOT NULL,
    FamilyName VARCHAR (50) NOT NULL,
    PetCount   INTEGER      NOT NULL
                            DEFAULT (0) 
);
/* ***** (End) Create Table [Person] ***** */


/* ***** (Begin) Create Table [Student] ***** */
CREATE TABLE Student (
    StudentId   INTEGER PRIMARY KEY AUTOINCREMENT
                        NOT NULL,
    PersonId    INTEGER REFERENCES Person (PersonId) 
                        NOT NULL,
    CourseCount INTEGER NOT NULL
                        DEFAULT (0) 
);
/* ***** (End) Create Table [Student] ***** */

COMMIT TRANSACTION;
PRAGMA foreign_keys = on;
