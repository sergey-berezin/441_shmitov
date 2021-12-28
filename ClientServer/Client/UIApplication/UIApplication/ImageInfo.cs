using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System.Windows.Interop;
using System;
using WebStructures;


namespace UIApplication
{
    public class ImageInfo : INotifyPropertyChanged
    {
        private BitmapImage content;

        public ImageInfo(WebImageInfo imgInfo)
        {
            FullName = imgInfo.FullName;
            Name = imgInfo.Name;
            RecognizedObjects = imgInfo.RecognizedObjects;
            Bitmap = new Bitmap(new MemoryStream(imgInfo.ByteContent));
            Content = ConvertToBitmapImage(Bitmap);
            Id = imgInfo.Id;
        }

        public ImageInfo(string fullName, List<KeyValuePair<string, double>> categories, byte[] byteArray, int id)
        {
            FullName = fullName;
            Name = FullName.Substring(FullName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            RecognizedObjects = categories;
            Bitmap = new Bitmap(new MemoryStream(byteArray));
            Content = ConvertToBitmapImage(Bitmap);
            Id = id;
        }

        public ImageInfo(Bitmap bitmap) 
        {
            Content = ConvertToBitmapImage(bitmap);
            RecognizedObjects = new();
        }

        public BitmapImage Content
        {
            get => content;
            set
            {
                if (content != value)
                {
                    content = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
                }
            }
        }

        public Bitmap Bitmap { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public List<KeyValuePair<string, double>> RecognizedObjects { get; set; }

        public int Id { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            string result = "Name: " + Name + "Objects:\n";
            if (RecognizedObjects.Count > 0)
                result += string.Join('\n', RecognizedObjects.Select(pair => $"{pair.Key}: {pair.Value}"));
            return result;
        }

        private BitmapImage ConvertToBitmapImage(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            BitmapImage bitmapImage;
            using (var memoryStream = new MemoryStream())
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);
                bitmapImage = new BitmapImage();
                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }
    }    
}
