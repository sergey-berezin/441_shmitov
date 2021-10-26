using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using YOLOv4MLNet.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using System.Collections.Concurrent;

namespace MainLib
{
    public class PictPredClass
    {

        // model is available here:
        // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4
        const string modelPath = "/Users/lucius/Desktop/ber_lab_1/YOLOv4MLNet/yolov4.onnx";

        const string imageFolder = "/Users/lucius/Desktop/ber_lab_1/YOLOv4MLNet/YOLOv4MLNet/Assets/Images";

        const string imageOutputFolder = "/Users/lucius/Desktop/ber_lab_1/YOLOv4MLNet/YOLOv4MLNet/Assets/Output";

        private CancellationTokenSource cancellationTokenSource;

        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike",
            "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign",
            "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear",
            "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee",
            "Anton_Bodrov_legs", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove",
            "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife",
            "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog",
            "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet",
            "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven",
            "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

        public PictPredClass()
        {
            model = ModelRecognition();
            Results = new BlockingCollection<SummaryClass>();
        }

        private BlockingCollection<SummaryClass> Results;
        private TransformerChain<OnnxTransformer> model;

        public TransformerChain<OnnxTransformer> ModelRecognition()
        {
            MLContext mlContext = new MLContext();


            // Define scoring pipeline
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
            model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));

            return model;
        }

        public string ImageOutputFolder { get; private set; }

        public void CancellationToken()
        {
            cancellationTokenSource.Cancel();
        }

        public async IAsyncEnumerable<SummaryClass> FindNames(string dir)
        {
 
            ImageOutputFolder = Path.Combine(dir, "OutputFolder");
            Directory.CreateDirectory(ImageOutputFolder);

            List<Task<SummaryClass>> processors = new List<Task<SummaryClass>>();
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            foreach (string imageName in Directory.GetFiles(dir))
            {
                if (!cancellationToken.IsCancellationRequested)
                {

                    Task<SummaryClass> task = PictTask(imageName);
                    processors.Add(task);
                }
                else
                    break;
            }

            for (int i = 0; i < processors.Count; i++)
            {
                if (!cancellationToken.IsCancellationRequested)
                    // the expression is blocked until any element appears in collection
                    yield return Results.Take();
                else
                    break;
            }

            await Task.WhenAll(processors);
            Results.Dispose();
        }

        public async Task<SummaryClass> PictTask(string imageName)
        {

            return await Task.Factory.StartNew(() =>
            {
                var labels = TakeNames(imageName);
                SummaryClass Result = new SummaryClass();
                Result.Name = imageName.Substring(imageName.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                Result.CatNames = labels;
                Results.Add(Result);
                return Result;
            });
        }

        private IReadOnlyList<string> TakeNames(string imageName)
        {

            List<string> Names = new List<string>();

            MLContext mlContext = new MLContext();

            var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);

            using (var bitmap = new Bitmap(Image.FromFile(Path.Combine(imageFolder, imageName))))
            {
                // predict
                var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                var results = predict.GetResults(classesNames, 0.3f, 0.7f);
                imageName = imageName.Substring(imageName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                using (var g = Graphics.FromImage(bitmap))
                {
                    foreach (var res in results)
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
                        Names.Add(res.Label);
                    }
                    bitmap.Save(Path.Combine(imageOutputFolder, Path.ChangeExtension(imageName, "_processed" + Path.GetExtension(imageName))));

                }
            }

            return Names;

        }
    }
}