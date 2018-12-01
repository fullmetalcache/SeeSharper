using System;
using System.Threading;
using System.Windows.Forms;

namespace SeeSharper
{
    class Program
    {
        static void Main(string[] args)
        {
            WebShot webshot = new WebShot();

            webshot.ScreenShot("https://www.blackhillsinfosec.com");
            webshot.ScreenShot("https://google.com");
            webshot.ScreenShot("http://www.reddit.com");
        }
    }
}
