using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace WebService.Controllers
{
    /// <summary>
    /// Główny kontroler 
    /// </summary>
    public class HomeController : Controller
    {
        private static Random random = new Random((int)DateTime.Now.Ticks); 
        /// <summary>
        /// Funkcja losująca stringa o podanej długości.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        
        //
        // GET: /Home/
        /// <summary>
        /// Akcja zwracająca widok strony głównej
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Wywoływana przez ajax metoda, pobierająca nazwę ścieżki naszego obrazka, która wywołuje funkcję UploadFile, a na końcu zwraca ściężkę do przyciętego pliku.
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public string SendImage(FormCollection form)
        {
            // ajax
            string imagepath = form["imagepath"];
            // Web Service function
            System.Drawing.Image returnedImage = UploadFile(HttpRuntime.AppDomainAppPath + "" + imagepath);
            
            // convert from Image to Bitmap
            Bitmap bitmap = new Bitmap(returnedImage);

            // returned image path
            string newFilePath = "Storage\\" + RandomString(8) + ".jpg";
            
            // save image to new path
            bitmap.Save(HttpRuntime.AppDomainAppPath + newFilePath);
            return newFilePath;
        }
        /// <summary>
        /// Funkcja przyjmująca ścieżkę do pliku. Otwierająca obrazek oraz przesyłająca go do WebSerive'u, a następnie odbierająca zwrócony, przycięty obrazek. 
        /// Funkcja zwraca obrazek
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public System.Drawing.Image UploadFile(string filename)
        {

                // get the exact file name from the path
                String strFile = System.IO.Path.GetFileName(filename);

                // create an instance to the web service
                Service.WebService1SoapClient srv = new Service.WebService1SoapClient();

                // get the file information form the selected file
                FileInfo fInfo = new FileInfo(filename);

                // get the length of the file to see if it is possible
                // to upload it
                long numBytes = fInfo.Length;
                double dLen = Convert.ToDouble(fInfo.Length / 1000000);


                Image returnImage = Image.FromFile(filename); 
                if (dLen < 4)
                {
                    // set up a file stream and binary reader for the
                    // selected file
                    FileStream fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fStream);

                    // convert the file to a byte arraBy
                    byte[] data = br.ReadBytes((int)numBytes);
                    br.Close();

                    // pass the byte array (file) and file name to the web service
                    byte[] sTmp = srv.UploadFile(data, strFile);
                    MemoryStream ms = new MemoryStream(sTmp);
                    returnImage = Image.FromStream(ms);
                    fStream.Close();
                    fStream.Dispose();
                    ms.Close();
                    // this will always say OK unless an error occurs,
                    // if an error occurs, the service returns the error message
                }
                return returnImage;
        }
    }
}
