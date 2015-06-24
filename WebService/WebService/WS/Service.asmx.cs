using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WS
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        [WebMethod]
        public int HelloWorld(int a, int b)
        {
            return a + b;
        }

        /// <summary>
        /// Metoda przyjmująca tablicę typu byte oraz nazwę pliku. Celem metody jest przycięcie wywołanie funkcji CropImage oraz zwrócenie do aplikacji internetowej przyciętego obrazka.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [WebMethod]
        public byte[] UploadFile(byte[] f, string fileName)
        {
                // instance a memory stream and pass the
                // byte array to its constructor
                MemoryStream ms = new MemoryStream(f);

                // instance a filestream pointing to the
                // storage folder, use the original file name
                // to name the resulting file

                System.Drawing.Image Image = System.Drawing.Image.FromStream(ms);
                System.Drawing.Image newImage = CropImage(Image); 

                // write the memory stream containing the original
                // file as a byte array to the filestream
              //  ms.WriteTo(fs);

                // clean up
                ms.Close();

                MemoryStream ms2 = new MemoryStream();
                newImage.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);
                ms2.Close();
                return ms2.ToArray();
        }

        /// <summary>
        /// Celem metody jest przykadrowanie otrzymanego obrazka, tak aby widok zawierał jedynie kartkę papieru, oraz zwrócenie go do metody UploadFile
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public System.Drawing.Image CropImage(System.Drawing.Image img)
        {
            //DocumentSkewChecker skewChecker = new DocumentSkewChecker();
            //double angle = skewChecker.GetSkewAngle(image);
            //RotateBilinear rotationFilter = new RotateBilinear(-angle);
            //rotationFilter.FillColor = Color.White;
            //Bitmap rotatedImage = rotationFilter.Apply(image);


            Bitmap image = new Bitmap(img);

            UnmanagedImage grayImage = null;

            if (image.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                grayImage = UnmanagedImage.FromManagedImage(image);
            }
            else
            {
                grayImage = UnmanagedImage.Create(image.Width, image.Height,
                    PixelFormat.Format8bppIndexed);
                Grayscale.CommonAlgorithms.BT709.Apply(UnmanagedImage.FromManagedImage(image), grayImage);
            }
            
            CannyEdgeDetector edgeDetector = new CannyEdgeDetector();
            UnmanagedImage edgesImage = edgeDetector.Apply(grayImage);

            OtsuThreshold thresholdFilter = new OtsuThreshold();
            thresholdFilter.ApplyInPlace(edgesImage);

            Dilatation DilatationFilter = new Dilatation();
            DilatationFilter.Apply(edgesImage);

            Opening OpeningFilter = new Opening();
            OpeningFilter.Apply(edgesImage);

            BlobCounter blobCounter = new BlobCounter();
            blobCounter.MinHeight = 32;
            blobCounter.MinWidth = 32;
            blobCounter.FilterBlobs = true;
            blobCounter.ObjectsOrder = ObjectsOrder.Size;

            blobCounter.ProcessImage(edgesImage);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            ExtractBiggestBlob BiggestBlob = new ExtractBiggestBlob();
            Bitmap biggestBlobsImage = BiggestBlob.Apply(edgesImage.ToManagedImage());
            AForge.IntPoint BiggestBlogCorners = BiggestBlob.BlobPosition;

            Crop cropFilter = new Crop(new Rectangle(BiggestBlogCorners.X, BiggestBlogCorners.Y, biggestBlobsImage.Width, biggestBlobsImage.Height));
            Bitmap croppedImage = cropFilter.Apply(image);
            return croppedImage;
        }
        private System.Drawing.Point[] ToPointsArray(List<AForge.IntPoint> points)
        {
            return points.Select(p => new System.Drawing.Point(p.X, p.Y)).ToArray();
        }

    }
}
