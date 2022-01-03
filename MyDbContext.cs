using Face_rec.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Face_rec
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions options) : base(options)
        { }

        public DbSet<Person> Persons { get; set; }
        public DbSet<PersonAudio> PersonAudios { get; set; }
    }
}
