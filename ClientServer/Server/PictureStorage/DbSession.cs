using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.IO;
using ImageStorage;
using System.Drawing;
using DbTableEntities;
using WebStructures;

namespace ImageStorage
{
    public static class DbClient
    {
        public static WebImageInfo Record(WebImageInfo pictureInfo)
        {
            byte[] byteArray = pictureInfo.ByteContent;
            string hashCode =
                string.Join("", new MD5CryptoServiceProvider()
                      .ComputeHash(byteArray)
                      .Select(byteEl => byteEl.ToString("X2")));

            using (var db = new ImageLibraryContext())
            {
                if (!IsExist(db, byteArray, hashCode))
                {
                    List<RecognizedCategory> recognizedObjects = new List<RecognizedCategory>();
                    foreach (var category in pictureInfo.RecognizedObjects)
                    {
                        var recObj = new RecognizedCategory()
                        {
                            Name = category.Key,
                            Confidence = category.Value
                        };
                        recognizedObjects.Add(recObj);
                        db.Add(recObj);
                    }

                    ImageInformation imageInfo = new ImageInformation()
                    {
                        Name = pictureInfo.FullName,
                        Hash = hashCode,
                        ImageDetails = new ImageDetails() { Content = byteArray },
                        RecognizedCategories = recognizedObjects
                    };
                    db.Add(imageInfo);

                    db.SaveChanges();
                }
            }

            return pictureInfo;
        }

        public static IEnumerable<WebImageInfo> SelectData()
        {
            using (var db = new ImageLibraryContext())
            {
                foreach (var pictureInfo in db.ImagesInfo.AsEnumerable())
                {
                    db.Entry(pictureInfo).Collection(picInfo => picInfo.RecognizedCategories).Load();
                    db.Entry(pictureInfo).Reference(picInfo => picInfo.ImageDetails).Load();
                    var pictureName = pictureInfo.Name;
                    var recognizedCategories = new List<KeyValuePair<string, double>>(
                        pictureInfo.RecognizedCategories.Select(obj => new KeyValuePair<string, double>(obj.Name, obj.Confidence)));

                    string fullName = pictureInfo.Name;
                    string name = fullName.Substring(fullName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    WebImageInfo webImgInfo = new()
                    {
                        Name = name,
                        FullName = fullName,
                        RecognizedObjects = recognizedCategories,
                        ByteContent = pictureInfo.ImageDetails.Content,
                        Id = pictureInfo.Id,
                    };
                    yield return webImgInfo;
                }
            }
        }

        public static WebImageInfo SelectObject(int id)
        {
            using var db = new ImageLibraryContext();
            var pictureInfo = db.ImagesInfo.Where(imgInfo => imgInfo.Id == id).FirstOrDefault();
            db.Entry(pictureInfo).Collection(picInfo => picInfo.RecognizedCategories).Load();
            db.Entry(pictureInfo).Reference(picInfo => picInfo.ImageDetails).Load();
            var pictureName = pictureInfo.Name;
            var recognizedCategories = new List<KeyValuePair<string, double>>(
                pictureInfo.RecognizedCategories.Select(obj => new KeyValuePair<string, double>(obj.Name, obj.Confidence)));

            string fullName = pictureInfo.Name;
            string name = fullName.Substring(fullName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            WebImageInfo webImgInfo = new()
            {
                Name = name,
                FullName = fullName,
                RecognizedObjects = recognizedCategories,
                ByteContent = pictureInfo.ImageDetails.Content,
                Id = pictureInfo.Id,
            };
            return webImgInfo;
        }

        public static int RemoveItem(int pictureInfoId)
        {
            using (var db = new ImageLibraryContext())
            {
                var imageInfo = db.ImagesInfo.Include(picInfo => picInfo.RecognizedCategories)
                                             .Include(picInfo => picInfo.ImageDetails)
                                             .Where(picInfo => picInfo.Id == pictureInfoId)
                                             .FirstOrDefault();
                if (imageInfo != null)
                {
                    db.Remove(imageInfo);
                    db.SaveChanges();
                }
            }
            return pictureInfoId;
        }

        private static bool IsExist(ImageLibraryContext db, byte[] imgByteArray, string imgHashCodeStr)
        {
            bool repeated = false;
            var similarImgs = db.ImagesInfo.Where(info => info.Hash.Equals(imgHashCodeStr));
            if (similarImgs != null)
            {
                foreach (var img in similarImgs)
                {
                    db.Entry(img).Reference(imgInfo => imgInfo.ImageDetails).Load();
                    repeated = Enumerable.SequenceEqual(img.ImageDetails.Content, imgByteArray);
                    if (repeated)
                        break;
                }
            }
            return repeated;
        }
    }
}
