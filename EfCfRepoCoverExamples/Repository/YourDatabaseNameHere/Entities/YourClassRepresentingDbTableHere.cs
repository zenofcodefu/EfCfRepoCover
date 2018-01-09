using System;
using System.ComponentModel.DataAnnotations;

namespace EfCfRepoCoverExamples.Repository.YourDatabaseNameHere.Entities
{
    public class YourClassRepresentingDbTableHere
    {
        [Key]
        public long MyTablePrimaryKey { get; set; }
        
        public string MyTableFirstName { get; set; }

        public string MyTableLastName { get; set; }

        public DateTime MyTableLastUpdatedDateTimeUtc { get; set; }
    }
}
