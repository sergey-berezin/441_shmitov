using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DbTableEntities
{
    public class ImageInformation
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Hash { get; set; }

        virtual public ImageDetails ImageDetails { get; set; }

        virtual public ICollection<RecognizedCategory> RecognizedCategories { get; set; }
    }
}
