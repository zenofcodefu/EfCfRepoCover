using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EfCfRepoCoverExamples.Repository.EfCodeFirstLibDb.Entities
{
    [Table("Person")]
    //[Table("dbo.Person")]
    public class Person
    {
        public Person()
        {
            PetCount = 0;
        }

        [Key]
        public int PersonId { get; set; }

        public string FirstName { get; set; }

        public string FamilyName { get; set; }

        public int PetCount { get; set; }
    }
}
