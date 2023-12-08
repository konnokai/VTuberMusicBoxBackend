using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

#nullable disable

namespace VTuberMusicBoxBackend.Models.Database
{
    public class MainDbContext : DbContext
    {
        public DbSet<Category> Category { get; set; }
        public DbSet<Track> Track { get; set; }

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // https://stackoverflow.com/a/64339484
            var dicValueComparer = new ValueComparer<Dictionary<string, ushort>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c);

            // https://www.conradakunga.com/blog/saving-collections-of-primitives-in-entity-framework-core/
            // Configure the value converter for the Dictionary<string, ushort>
            modelBuilder.Entity<Category>()
                .Property(x => x.VideoIdList)
                .HasConversion(new ValueConverter<Dictionary<string, ushort>, string>(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, ushort>>(v)))
                .Metadata.SetValueComparer(dicValueComparer);
        }
    }
}
