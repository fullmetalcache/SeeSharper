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
        private static Object _lockObject = new Object();
        private int _threadsActive = 0;
        private int _maxThreads;

        public WebShot( int maxThreads )
        {
            _maxThreads = maxThreads;
        }

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
            string tempFile = url.Replace("://", "_");
            tempFile = tempFile.Replace(".", "_");

            File.WriteAllText( tempFile + ".html", responseFromServer);

            int width = 1024;
            int height = 768;

            var th = new Thread(() =>
            {
                WebBrowser browser = new WebBrowser();
                browser.ScriptErrorsSuppressed = true;
                browser.Width = width;
                browser.Height = height;
                browser.DocumentCompleted += OnDocumentCompleted;
                string curDir = Directory.GetCurrentDirectory();
                Uri uri = new Uri(String.Format("file:///{0}/{1}.html", curDir, tempFile));
                browser.Name = tempFile;
                browser.Navigate(uri);
                Application.Run();
            });

            th.SetApartmentState(ApartmentState.STA);

            while( _threadsActive >= _maxThreads )
            {
                Thread.Sleep(500);
            }

            lock (_lockObject)
            {
                _threadsActive += 1;
            }

            th.Start();
        }


        void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser browser = (WebBrowser)sender;
            using (Graphics graphics = browser.CreateGraphics())
            using (Bitmap bitmap = new Bitmap(browser.Width, browser.Height, graphics))
            {
                Rectangle bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                browser.DrawToBitmap(bitmap, bounds);
            
                Bitmap resized = new Bitmap(bitmap, new Size(bitmap.Width, bitmap.Height));
                String filename = String.Format("{0}.jpeg", browser.Name);
                resized.Save(filename, ImageFormat.Jpeg);
            }
            Console.WriteLine(browser.Name);
            string curDir = Directory.GetCurrentDirectory();
            File.Delete(String.Format("{0}/{1}.html", curDir, browser.Name));

            lock (_lockObject)
            {
                _threadsActive -= 1;
            }

            Application.ExitThread();
        }
    }
}
