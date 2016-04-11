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

namespace LoadTestFaceAPI
{
    [TestClass]
    public class UnitTest1
    {   
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("########################");

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
            String searchFolder = @"D:\Face Detection Databases\ORL";
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            var files = GetFilesFromDirectory(searchFolder, filters, true);

            int numberOfFiles = files.Length;
            Random rnd = new Random();
            int randomImage = rnd.Next(0, numberOfFiles);

            string filePath = files[randomImage];

            if (context.Properties.Contains("$LoadTestUserContext")) //running as load test
                context.BeginTimer("MyTimerFaceDetection");
            
            FaceRectangle[] faceRects = await UploadAndDetectFaces(filePath);

            if (context.Properties.Contains("$LoadTestUserContext")) //running as load test
                context.EndTimer("MyTimerFaceDetection");

            Assert.IsTrue(faceRects.Length >= 0);
        }

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
