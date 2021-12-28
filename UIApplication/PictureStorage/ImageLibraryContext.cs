using DbTableEntities;
using Microsoft.EntityFrameworkCore;

namespace ImageStorage
{
    public class ImageLibraryContext : DbContext
    {
        // path to the db
        public static string DataBase =
            @"A:\4_year\.NET_Technologies\Projects\SladkiyZmiter\UIApplication\PictureStorage\pictures_lib.db";

        public DbSet<ImageInformation> ImagesInfo { get; set; }

        public DbSet<ImageDetails> ImagesDetails { get; set; }

        public DbSet<RecognizedCategory> RecognizedCategories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite($"Data Source={DataBase}");
        }
    }
}
