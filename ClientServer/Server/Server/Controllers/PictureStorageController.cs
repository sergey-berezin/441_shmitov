using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Drawing;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebStructures;
using ImageStorage;
using DbTableEntities;
using ClassLib;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PictureStorageController : Controller
    {
        [HttpGet]
        public ActionResult<List<WebImageInfo>> GetImagesInfo()
        {
            List<WebImageInfo> result = null;
            try
            {
                result = new List<WebImageInfo>(DbClient.SelectData());
            }
            catch (Exception)
            {
                return StatusCode(503, "Service is unavaivable");
            }
            return result;
        }

        [HttpGet("{id:int}")]
        public ActionResult<KeyValuePair<byte[], List<RecognizedCategory>>> Get(int id)
        {
            var pictureInfo = DbClient.SelectObject(id);
            if (pictureInfo != null)
            {
                KeyValuePair<byte[], List<RecognizedCategory>> result = 
                    new(pictureInfo.ByteContent, new(pictureInfo.RecognizedObjects.Select(
                        obj => new RecognizedCategory() 
                        { 
                            Name = obj.Key, 
                            Confidence = obj.Value, 
                            ImageInfoId = id 
                        }
                    )));
                return result;
            }
            return NotFound("Image information with given id is not found");
        }

        [HttpPost]
        public async Task<ActionResult<WebImageInfo>> StartRecognition(KeyValuePair<string, byte[]> image)
        {
            string imageName = image.Key.Substring(image.Key.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            PicProcessing pictureProcessing = new PicProcessing();
            var result = await pictureProcessing.OnePictureProcessing(new(new MemoryStream(image.Value)), imageName);
            var webImgInfo = result?.ToWebImageInfo(image.Value);
            if (webImgInfo != null)
            {
                DbClient.Record(webImgInfo);
            }
            return webImgInfo;
        }

        [HttpDelete("{imageInfoId:int}")]
        public int DeleteImageInfo(int imageInfoId)
        {
            var index = DbClient.RemoveItem(imageInfoId);
            return index;
        }
    }


    public static class ProcessResultExtension
    {
        public static WebImageInfo ToWebImageInfo(this PictureResults results, byte[] content)
        {
            WebImageInfo webImgInfo = new();
            webImgInfo.FullName = results.ImageName;
            webImgInfo.Name = webImgInfo.FullName.Substring(webImgInfo.FullName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            webImgInfo.RecognizedObjects = new();
            webImgInfo.ByteContent = ChangeBitmap(content, results, webImgInfo);
            return webImgInfo;
        }

        private static byte[] ChangeBitmap(byte[] content, PictureResults results, WebImageInfo webImgInfo)
        {
            Bitmap bitmap = new(new MemoryStream(content));
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
                    webImgInfo.RecognizedObjects.Add(new KeyValuePair<string, double>(res.Label, res.Confidence));
                }
            }
            return (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[]));
        }

    }

}
