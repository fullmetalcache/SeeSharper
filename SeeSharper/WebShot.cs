/***
 * File: WebShot.cs
 * Author: Brian Fehrman (fullmetalcache)
 * Date: 2018-12-27
 * Description:
 *      Contains the code for taking screenshots of websites.
 *      Is slightly round-about to allow for ignoring certificate errors
 *      and not needing any third-party dependencies. Basic explanation follows.
 *      
 *      WebRequest class respects the settings of ServicePointManager.ServerCertificateValidationCallback,
 *      which allows for ignoring certificate errors. The WebRequest class, however,
 *      does not have a way to take a screenshot.
 *      
 *      The WebBrowser class allows for taking screenshots but does not allow for ignoring certficate errors.
 *      
 *      The solution here is to combine both WebRequest and WebBrowser to be able to take screenshots
 *      of websites and ignore certificate errors. WebRequest is used to make the initial request to get
 *      the HTML from the site. The HTML that was retrieved by WebRequest is then saved to a local .html
 *      file. The WebBrowser class is then used to load the local .html file, render the code, and 
 *      then take a screenshot of the rendered code. The local .html file is deleted after the rendered
 *      code has been transformed into an image.
 */

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
        private int _maxThreads;
        private int _threadsActive = 0;

        public WebShot( int maxThreads )
        {
            _maxThreads = maxThreads;
        }

        /// <summary>
        /// Function to create a screenshot of a given website
        /// </summary>
        /// <param name="url">URL of Site to Screenshot</param>
        public void ScreenShot(string url)
        {
            //Screenshot Width and Height
            int width = 1024;
            int height = 768;

            string tempFile = getHTML(url);
            if (tempFile != "")
            {
                screenshotFile(tempFile, width, height);

            }
        }
        /// <summary>
        /// Retrieves the HTML from a given URL, saves it to a file,
        /// and then returns the name of the file. SSL Certificate
        /// errors are ignored
        /// </summary>
        /// <param name="url">URL to retrieve</param>
        /// <returns>Name of file to which HTML was saved</returns>
        private string getHTML(string url)
        {
            //This line is needed to ignore cert errors
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            //Create a webrequest, get the response, convert the response into HTML text
            WebRequest request = WebRequest.Create(url);
            request.Timeout = 15000;
            string responseFromServer = "";

            try
            {
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
            }
            catch
            {
                Console.WriteLine(String.Format("Timeout or Error reaching:{0}", url));
                return "";
            }

            //Write HTML text to a file
            string tempFile = url.Replace("://", "_");
            tempFile = tempFile.Replace(".", "_");
            File.WriteAllText(tempFile + ".html", responseFromServer);

            return tempFile;
        }

        /// <summary>
        /// Loads HTML from the given file, renders the HTML, and then calls
        /// a callback function to screenshot the rendered HTML and save the
        /// resulting image to a file. Note that the HTML file is deleted
        /// after the image file is created.
        /// </summary>
        /// <param name="fileName">Name of file to load, render, and screenshot</param>
        /// <param name="width">Width of screenshot</param>
        /// <param name="height">Height of screenshot</param>
        private void screenshotFile( string fileName, int width, int height )
        {
            //Create a thread to load and render the saved HTML file.
            //Actual screenshotting takes place the OnDocumentCompleted callback function
            //that is added in this thread
            var th = new Thread(() =>
            {
                //Set resolution and ignore JavaScript errors in the page
                WebBrowser browser = new WebBrowser();
                browser.ScriptErrorsSuppressed = true;
                browser.Width = width;
                browser.Height = height;

                //Add a callback function for when the site has been fully rendered
                //This callback function is where the actual screenshotting takes place
                browser.DocumentCompleted += OnDocumentCompleted;

                //Open the saved HTML file and render it
                string curDir = Directory.GetCurrentDirectory();
                Uri uri = new Uri(String.Format("file:///{0}/{1}.html", curDir, fileName));
                browser.Name = fileName;
                browser.Navigate(uri);

                //Forces thread to wait until Application.ExitThread() is called in the 
                //OnDocumentCompleted callback function. This ensures that the WebBrowser
                //object is not destroyed until after it is consumed by the OnDocumentCompleted
                //callback function.
                Application.Run();
            });

            //Set to Single Threaded Application (STA) to all for the threading to work correctly
            th.SetApartmentState(ApartmentState.STA);

            //Wait if we have reached the maximum number of active threads
            while (_threadsActive >= _maxThreads)
            {
                Thread.Sleep(500);
            }

            //Updated the number of active threads
            lock (_lockObject)
            {
                _threadsActive += 1;
            }

            //Start the latest thread
            th.Start();
        }

        /// <summary>
        /// Callback function for WebBrowser DocumentCompleted event. Once the local HTML file has been
        /// loaded and rendered, this function will be called to take and save a screenshot of the rendered
        /// site. Note that the HTML file is deleted after the image file is created.
        /// </summary>
        /// <param name="sender">Object that generated the event that called this callback function</param>
        /// <param name="e">Additional arguments passed to the callback function</param>
        void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //Get WebBrowser object, create a new bitmap object, set resolution,
            //set filetype (JPEG), and save the file.
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

            //Delete the temporary HTML file that was created in the ScreenShot method
            string curDir = Directory.GetCurrentDirectory();
            File.Delete(String.Format("{0}/{1}.html", curDir, browser.Name));

            //Decrement the number of active threads
            lock (_lockObject)
            {
                _threadsActive -= 1;
            }

            //Allow the thread to exit and the objects to be destroyed
            Application.ExitThread();
        }
    }
}
