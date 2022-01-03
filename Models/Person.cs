using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Face_rec.Models
{
    public class Person
    {

        [Key]
        public int TableId { get; set; }
        public string PersonId { get; set; }
        public string Name { get; set;}
        public string UserData { get; set; }
        public string LoggedIn { get; set; }
        public string LoggedOut { get; set; }
    }


}
