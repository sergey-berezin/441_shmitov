using System;
using YOLOv4MLNet.DataStructures;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace ClassLib
{
    public class PicProcessing
    {
        private CancellationTokenSource cancellationSource = new CancellationTokenSource();
        public TransformerChain<OnnxTransformer> ModelCreation()
        {
            var mlContext = new MLContext();
            // path to the model
            string modelPath = @"A:\4_year\.NET_Technologies\Projects\441_abramov\UIApp\ParallelYOLOv4\YOLOv4Model\yolov4.onnx";
            // Create prediction engine
            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));

            // Fit on empty list to obtain input data schema
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));
            return model;

        }

        public void Cancel()
        {
            cancellationSource.Cancel();
            //а почему нельзя передавать CancellationToken из Program.cs?
        }

        public IEnumerable<PictureResults> ObjectDetecting(string ImFolder)
        {
            List<string> files = new(Directory.GetFiles(ImFolder));
            List<Task<IReadOnlyList<YoloV4Result>>> tasks = new();
            var model = ModelCreation();

            foreach (string imageName in files)
            {
                if (!cancellationSource.Token.IsCancellationRequested)
                {
                    Task<IReadOnlyList<YoloV4Result>> one_image = OnePictureProcessing(imageName, model);
                    tasks.Add(one_image);
                }
                else
                {
                    break;
                }
            }

            while (tasks.Count > 0 && !cancellationSource.IsCancellationRequested)
            {
                int taskId = Task.WaitAny(tasks.ToArray());
                string file = files[taskId];
                yield return new PictureResults(tasks[taskId].Result, files[taskId]);
                tasks.RemoveAt(taskId);
                files.RemoveAt(taskId);
            }
        }

        public async Task<IReadOnlyList<YoloV4Result>> OnePictureProcessing(string filename, TransformerChain<OnnxTransformer> model)
        {            
            return await Task.Factory.StartNew(() =>
            {
                string imageFolder = filename.Substring(0, filename.LastIndexOf(Path.DirectorySeparatorChar));
                var mlContext = new MLContext();
                string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };
                var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);

                using (var bitmap = new Bitmap(Image.FromFile(filename)))
                {
                    // predict
                    var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                    var results = predict.GetResults(classesNames, 0.3f, 0.7f);
                    return results;
                }
            });
        }
    }


    public class PictureResults
    {
        public PictureResults(IReadOnlyList<YoloV4Result> iRes, string iName)
        {
            ReconizedObjects = new List<YoloV4Result>(iRes);
            ImageName = iName;
        }

        public List<YoloV4Result> ReconizedObjects { get; }
        public string ImageName { get; }
    }


}
