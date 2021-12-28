using System;
using System.IO;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DbTableEntities;
using WebStructures;

namespace ClientLib
{
    public class ClientSession
    {
        public ClientSession() => Cancelled = false;

        public static string Url { get; } = "http://localhost:5000/api/picturestorage";

        public bool Cancelled { get; set; }

        public static List<WebImageInfo> Get()
        {
            string answer;
            List<WebImageInfo> queryResult;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    answer = client.GetStringAsync(Url).Result;
                    queryResult = JsonConvert.DeserializeObject<List<WebImageInfo>>(answer);
                }
                catch (Exception)
                {
                    queryResult = new List<WebImageInfo>();
                }
            }
            return queryResult;
        }

        public static KeyValuePair<byte[], List<RecognizedCategory>>? Get(int imgInfoId)
        {
            // byte[] obj is bitmap source. If you want to get System.Drawing.Bitmap
            // you need to read this byte array from memory stream
            #nullable enable
            string? answer = null;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    answer = client.GetStringAsync(Url + '/' + imgInfoId.ToString()).Result;
                }
                catch (Exception) { }
            }
            if (answer != null)
                return JsonConvert.DeserializeObject<KeyValuePair<byte[], List<RecognizedCategory>>>(answer);
            else
                return null;
        }

        public static int Delete(int index)
        {
            string answer;
            using (var client = new HttpClient())
            {
                try
                {
                    answer = client.DeleteAsync($"{Url}/{index}").Result.Content.ReadAsStringAsync().Result;
                }
                catch (Exception)
                {
                    return -1;
                }
            }
            return Convert.ToInt32(answer);
        }

        public IEnumerable<WebImageInfo?> PostAsync(string imageFolder)
        {
            using (var client = new HttpClient())
            {
                Bitmap bitmap;
                byte[] byteArray;
                KeyValuePair<string, byte[]> image;
                List<Task<HttpResponseMessage>> answers = new List<Task<HttpResponseMessage>>();
                var imageFiles = Directory.GetFiles(imageFolder);
                foreach (var imagePath in imageFiles)
                {
                    bitmap = new Bitmap(Image.FromFile(imagePath));
                    byteArray = (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[]));
                    image = new KeyValuePair<string, byte[]>(imagePath, byteArray);
                    var serializedImages = JsonConvert.SerializeObject(image);
                    var content = new StringContent(serializedImages);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    Task<HttpResponseMessage>? clientTask = null;
                    try
                    {
                        clientTask = client.PostAsync(Url, content);
                        answers.Add(clientTask);
                    }
                    catch (Exception)
                    {

                    }
                    if (clientTask == null)
                        break;
                }
                while (answers.Count > 0 && !Cancelled)
                {
                    int taskId = Task.WaitAny(answers.ToArray());
                    if (answers[taskId].Status != TaskStatus.Faulted)
                    {
                        HttpResponseMessage response = answers[taskId].Result;
                        string content = response.Content.ReadAsStringAsync().Result;
                        yield return JsonConvert.DeserializeObject<WebImageInfo>(content);
                    }
                    answers.RemoveAt(taskId);
                }
            }
        }

        public void Cancel()
        {
            if (!Cancelled)
            {
                Cancelled = true;
            }
        }
    }
}
