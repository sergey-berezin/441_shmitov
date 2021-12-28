using System.ComponentModel.DataAnnotations;

namespace DbTableEntities
{
    public class RecognizedCategory
    {
        [Key]
        public int ObjectId { get; set; }

        public int ImageInfoId { get; set; }

        public string Name { get; set; }

        public double Confidence { get; set; }
    }
}
