using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.IO;


namespace SeeSharper
{
    class WebShot
    {
        public void ScreenShot(string url)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();

            File.WriteAllText(@".\temppage.html", responseFromServer);

            int width = 800;
            int height = 600;

            using (WebBrowser browser = new WebBrowser())
            {
                browser.ScriptErrorsSuppressed = true;
                browser.Width = width;
                browser.Height = height;
                browser.ScrollBarsEnabled = true;

                // This will be called when the page finishes loading
                browser.DocumentCompleted += WebShot.OnDocumentCompleted;
                string curDir = Directory.GetCurrentDirectory();
                Uri uri = new Uri(String.Format("file:///{0}/temppage.html", curDir));
                browser.Navigate(uri);

                // This prevents the application from exiting until
                // Application.Exit is called
                Application.Run();
            }
        }

        static void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // Now that the page is loaded, save it to a bitmap
            WebBrowser browser = (WebBrowser)sender;
            using (Graphics graphics = browser.CreateGraphics())
            using (Bitmap bitmap = new Bitmap(browser.Width, browser.Height, graphics))
            {
                Rectangle bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                browser.DrawToBitmap(bitmap, bounds);
                bitmap.Save("screenshot1.bmp", ImageFormat.Bmp);
            }

            // Instruct the application to exit
            Application.Exit();
        }
    }
}
