using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordFrequency
{
    public class RecordContext : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<RecordContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public RecordContext(string connectionString) : base(new SQLiteConnection() { ConnectionString = connectionString }, true)
        {
        }

        public DbSet<Record> Records { get; set; }

        public DbSet<Stat> Stats { get; set; }
    }

    public class Record 
    {
        public int Id { get; set; }
        [Index]
        public string Word { get; set; }
    }

    public class Stat
    {
        public int Id { get; set; }
        [Index]
        public string Word { get; set; }
        public int Count { get; set; }
    }
}
