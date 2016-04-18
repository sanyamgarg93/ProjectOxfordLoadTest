using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;
using System.Threading;

namespace LoadTestFaceAPI
{
    //Class for constants
    public static class Constants
    {
        public static string FACE_API_KEY 
        {
            get { return "#############################"; } //Replace with your Subscription Key
        }

        public static string ImageDirectory
        {
            get { return @"D:\Face Detection Databases\CaltechFaces"; } //Replace with the directory containing images.
        }
    }

    [TestClass]
    public class UnitTest1
    {
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient(Constants.FACE_API_KEY);

        public TestContext TestContext
        {
            get { return context; }
            set { context = value; }
        }
        private TestContext context;

        [TestMethod]
        public async Task TestFaceDetection()
        {
            this.context = TestContext;

            //Extract all image files' location from a given folder
            String searchFolder = Constants.ImageDirectory;
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            var files = GetFilesFromDirectory(searchFolder, filters, true);
            int numberOfFiles = files.Length;

            //Return a random image location 
            Random rnd = new Random();
            int randomImage = rnd.Next(0, numberOfFiles);
            string filePath = files[randomImage];

            if (context.Properties.Contains("$LoadTestUserContext")) //Begin timing load test
                context.BeginTimer("MyTimerFaceDetection");
            
            //Detect faces in the selected image
            FaceRectangle[] faceRects = { };
            Task.Run(async () =>
            {
                faceRects = await UploadAndDetectFaces(filePath);
            }).GetAwaiter().GetResult();             
                        
            if (context.Properties.Contains("$LoadTestUserContext")) //End timing load test
                context.EndTimer("MyTimerFaceDetection");

            Assert.IsTrue(faceRects.Length > 0); //Since all images contain detectable frontal faces
        }

        // Method returns an array of file locations present in the searchFolder location
        public static String[] GetFilesFromDirectory(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }

        // Face detection procedure that uploads images and retuns face positions 
        private async Task<FaceRectangle[]> UploadAndDetectFaces(string imageFilePath)
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);                    
                    var faceRects = faces.Select(face => face.FaceRectangle);
                    return faceRects.ToArray();
                }
            }
            catch (Exception)
            {
                return new FaceRectangle[0];
            }
        }
    }
}
