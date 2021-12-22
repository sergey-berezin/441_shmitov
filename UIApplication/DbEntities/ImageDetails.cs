using System.ComponentModel.DataAnnotations;

namespace DbTableEntities
{
    public class ImageDetails
    {
        [Key]
        public int ImageDetailsId { get; set; }

        public int ImageInfoId { get; set; }

        public byte[] Content { get; set; }
    }
}
