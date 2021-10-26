using System;
using System.Threading.Tasks;
using MainLib;

namespace MainProject
{
    class MainClass
    {
        static async Task Main(string[] args)
        {
            string ImageFolder;
            Console.WriteLine("Please type path to the image folder");
            ImageFolder = Console.ReadLine();

            PictPredClass obj = new PictPredClass();
            await foreach (var processResult in obj.FindNames(ImageFolder))
                Console.WriteLine(processResult);

        }
    }
}
