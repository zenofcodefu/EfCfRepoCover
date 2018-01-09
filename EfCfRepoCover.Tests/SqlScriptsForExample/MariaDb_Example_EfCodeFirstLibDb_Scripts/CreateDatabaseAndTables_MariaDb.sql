
/* ***** (Begin) Create Database [EfCodeFirstLibDb] ***** */
CREATE DATABASE IF NOT EXISTS `efcodefirstlibdb`;
/* ***** (End) Create Database [EfCodeFirstLibDb] ***** */

USE `efcodefirstlibdb`;

/* ***** (Begin) Create Table [Person] ***** */
CREATE TABLE IF NOT EXISTS `person` (
  `PersonId` int(20) NOT NULL AUTO_INCREMENT,
  `FirstName` varchar(50) NOT NULL,
  `FamilyName` varchar(50) NOT NULL,
  `PetCount` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`PersonId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/* ***** (End) Create Table [Person] ***** */


/* ***** (Begin) Create Table [Student] : Includes Foreign Key To [Student] table (based on [Person].[PersonId] and [Student].[PersonId] relationship) ***** */
CREATE TABLE IF NOT EXISTS `student` (
  `StudentId` int(20) NOT NULL AUTO_INCREMENT,
  `PersonId` int(20) NOT NULL,
  `CourseCount` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`StudentId`),
  KEY `person_personId` (`PersonId`),
  CONSTRAINT `person_personId` FOREIGN KEY (`PersonId`) REFERENCES `person` (`PersonId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/* ***** (End) Create Table [Student] : Includes Foreign Key To [Student] table (based on [Person].[PersonId] and [Student].[PersonId] relationship) ***** */

