using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Drawing;
using ClassLib;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System.Windows.Interop;
using System;


namespace UIApplication
{
    public class ImageInfo : INotifyPropertyChanged
    {
        private BitmapImage content;
        public ImageInfo(PictureResults results)
        {
            FullName = results.ImageName;
            Name = FullName.Substring(FullName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            RecognizedObjects = new();
            Bitmap = ReadBitmap(results);
                
            Content = ConvertToBitmapImage(Bitmap);
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
        
        private Bitmap ReadBitmap(PictureResults results)
        {
            Bitmap bitmap = new(FullName);
            using (var g = Graphics.FromImage(bitmap))
            {
                foreach (var res in results.ReconizedObjects)
                {
                    // draw predictions
                    var x1 = res.BBox[0];
                    var y1 = res.BBox[1];
                    var x2 = res.BBox[2];
                    var y2 = res.BBox[3];
                    g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                    using (var brushes = new SolidBrush(Color.FromArgb(50, Color.Red)))
                    {
                        g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                    }

                    g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                                    new Font("Arial", 12), Brushes.Blue, new PointF(x1, y1));
                    RecognizedObjects.Add(new(res.Label, res.Confidence));
                }
            }
            return bitmap;
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
