using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Face_rec.Models
{
    public class PersonAudio
    {
        [Key]
        public int TableId { get; set; }
        public string ProfileId { get; set; }
        public string Name { get; set; }
    }
}
