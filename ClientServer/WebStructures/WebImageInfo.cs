using System;
using System.Collections.Generic;

namespace WebStructures
{
    public class WebImageInfo
    {
        public string FullName { get; set; }

        public string Name { get; set; }

        public List<KeyValuePair<string, double>> RecognizedObjects { get; set; }

        public int Id { get; set; }

        public byte[] ByteContent { get; set; }
    }
}
