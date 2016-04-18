using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;

namespace ProjectOxfordFaceDetection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("22e501a9395f48728021aafa6db7efa0"); /* Replace with your own registration key*/

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            Enter the directory that contains the images in the formats mentioned in the filters array.  
            */
            String searchFolder = @"D:\Face Detection Databases\CaltechFaces";
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            var files = GetFilesFromDirectory(searchFolder, filters, true);
            
            /*
            Random file is chosen from the image directory
            */
            int numberOfFiles = files.Length;
            Random rnd = new Random();
            int randomImage = rnd.Next(0, numberOfFiles);

            string filePath = files[randomImage];

            /*
            Image is rendered on the Main Window
            */
            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = new Uri(filePath);   
            bitmapSource.EndInit();                     

            FacePhoto.Source = bitmapSource;

            /*
            Face detection method is called using the input image URI
            */
            Title = "Detecting...";
            FaceRectangle[] faceRects = await UploadAndDetectFaces(filePath);
            Title = String.Format("Detection Finished. {0} face(s) detected", faceRects.Length);

            drawFaceOnImage(bitmapSource, faceRects);
        }

        /*
        Method for drawing and displaying rectangles on the input image
        */
        public void drawFaceOnImage(BitmapImage bitmapSource, FaceRectangle[] faceRects)
        {
            if (faceRects.Length > 0)
            {
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource, new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                double resizeFactor = 96 / dpi;

                foreach (var faceRect in faceRects)
                {
                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red, 2),
                        new Rect(faceRect.Left * resizeFactor, faceRect.Top * resizeFactor, faceRect.Width * resizeFactor, faceRect.Height * resizeFactor)
                        );
                }

                drawingContext.Close();
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                faceWithRectBitmap.Render(visual);
                FacePhoto.Source = faceWithRectBitmap;
            }
        }

        /*
        The method descibes how to call the Face detection API of project oxford. The DetectAsync method takes 
        a stream of image pixel buffer as its argument and returns an array of face rectanges. 
        Code documentation: https://www.microsoft.com/cognitive-services/en-us/face-api/documentation/Face-API-How-to-Topics/HowtoDetectFacesinImage
        */
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

        /*
        Method for storing the URIs of all images in a string array present in the searchFolder directory.
        */
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
    }
}
