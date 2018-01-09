using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EfCfRepoCoverTests.Repository.EfCodeFirstLibDb.Entities
{
    [Table("Student")]
    //[Table("dbo.Student")]
    public class Student
    {
        public Student()
        {
            CourseCount = 0;
        }

        [Key]
        public int StudentId { get; set; }

        public int PersonId { get; set; }

        public int CourseCount { get; set; }
    }
}
